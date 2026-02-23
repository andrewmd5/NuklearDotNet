#nullable enable
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Numerics;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using ExampleShared;
using NuklearDotNet;
using Vortice.ShaderCompiler;
using Vortice.Vulkan;
using static Vortice.Vulkan.Vulkan;

namespace Example_Vulkan;

readonly record struct VulkanTexture(VkImage Image, VkDeviceMemory Memory, VkImageView View, VkDescriptorSet DescriptorSet);

unsafe sealed class VulkanDevice : NuklearDeviceTex<VulkanTexture>, INuklearDeviceRenderHooks
{
    readonly VkInstance _instance;
    readonly VkInstanceApi _instanceApi;
    readonly VkPhysicalDevice _physicalDevice;
    readonly VkDevice _device;
    readonly VkDeviceApi _api;
    readonly VkQueue _graphicsQueue;
    readonly VkQueue _presentQueue;
    readonly uint _graphicsFamily;
    readonly uint _presentFamily;
    readonly VkSurfaceKHR _surface;

    VkSwapchainKHR _swapchain;
    VkImage[] _swapImages = [];
    VkImageView[] _swapImageViews = [];
    VkFormat _swapFormat;
    uint _swapImageCount;

    readonly VkPipelineLayout _pipelineLayout;
    readonly VkPipeline _pipeline;
    readonly VkDescriptorSetLayout _descriptorSetLayout;
    readonly VkDescriptorPool _descriptorPool;
    readonly VkSampler _sampler;

    readonly VkCommandPool _commandPool;
    readonly VkCommandBuffer[] _commandBuffers;
    readonly VkFence[] _fences;
    readonly VkSemaphore[] _acquireSemaphores;
    readonly VkSemaphore[] _renderSemaphores;

    VkBuffer _vertexBuffer;
    VkDeviceMemory _vertexMemory;
    uint _vertexBufferSize;

    VkBuffer _indexBuffer;
    VkDeviceMemory _indexMemory;
    uint _indexBufferSize;

    VkCommandBuffer _cmd;
    uint _frameIndex;
    int _width;
    int _height;

    const string VertexShaderSource = """
        #version 450
        layout(push_constant) uniform PC { mat4 projection; };
        layout(location = 0) in vec2 inPos;
        layout(location = 1) in vec2 inUV;
        layout(location = 2) in vec4 inColor;
        layout(location = 0) out vec2 outUV;
        layout(location = 1) out vec4 outColor;
        void main() {
            gl_Position = projection * vec4(inPos, 0, 1);
            outUV = inUV;
            outColor = inColor;
        }
        """;

    const string FragmentShaderSource = """
        #version 450
        layout(set = 0, binding = 0) uniform sampler2D tex;
        layout(location = 0) in vec2 inUV;
        layout(location = 1) in vec4 inColor;
        layout(location = 0) out vec4 outColor;
        void main() {
            outColor = inColor * texture(tex, inUV);
        }
        """;

    public VulkanDevice(nint hwnd, nint hInstance, int width, int height)
    {
        _width = width;
        _height = height;

        vkInitialize().CheckResult();

        List<VkUtf8String> instanceExtensions = [VK_KHR_SURFACE_EXTENSION_NAME, VK_KHR_WIN32_SURFACE_EXTENSION_NAME];
        using VkStringArray vkExtensions = new(instanceExtensions);

        VkUtf8ReadOnlyString appName = "NuklearVulkan"u8;
        VkUtf8ReadOnlyString engineName = "NuklearDotNet"u8;
        VkApplicationInfo appInfo = new()
        {
            pApplicationName = appName,
            applicationVersion = new VkVersion(1, 0, 0),
            pEngineName = engineName,
            engineVersion = new VkVersion(1, 0, 0),
            apiVersion = VkVersion.Version_1_3
        };

        VkInstanceCreateInfo instanceCreateInfo = new()
        {
            pApplicationInfo = &appInfo,
            enabledExtensionCount = vkExtensions.Length,
            ppEnabledExtensionNames = vkExtensions
        };

        vkCreateInstance(&instanceCreateInfo, out _instance).CheckResult();
        _instanceApi = GetApi(_instance);

        VkWin32SurfaceCreateInfoKHR surfaceInfo = new()
        {
            hinstance = hInstance,
            hwnd = hwnd
        };
        _instanceApi.vkCreateWin32SurfaceKHR(&surfaceInfo, out _surface).CheckResult();

        uint deviceCount = 0;
        _instanceApi.vkEnumeratePhysicalDevices(&deviceCount, null).CheckResult();
        VkPhysicalDevice* physDevices = stackalloc VkPhysicalDevice[(int)deviceCount];
        _instanceApi.vkEnumeratePhysicalDevices(&deviceCount, physDevices).CheckResult();

        _physicalDevice = default;
        _graphicsFamily = uint.MaxValue;
        _presentFamily = uint.MaxValue;

        VkQueueFamilyProperties* qfProps = stackalloc VkQueueFamilyProperties[64];
        for (int d = 0; d < deviceCount; d++)
        {
            VkPhysicalDevice pd = physDevices[d];
            uint qfCount = 64;
            _instanceApi.vkGetPhysicalDeviceQueueFamilyProperties(pd, &qfCount, qfProps);

            uint gf = uint.MaxValue;
            uint pf = uint.MaxValue;
            for (uint i = 0; i < qfCount; i++)
            {
                if ((qfProps[i].queueFlags & VkQueueFlags.Graphics) != 0)
                    gf = i;

                VkBool32 presentSupport;
                _instanceApi.vkGetPhysicalDeviceSurfaceSupportKHR(pd, i, _surface, &presentSupport);
                if (presentSupport)
                    pf = i;

                if (gf != uint.MaxValue && pf != uint.MaxValue)
                    break;
            }

            if (gf != uint.MaxValue && pf != uint.MaxValue)
            {
                VkPhysicalDeviceVulkan13Features features13 = new();
                VkPhysicalDeviceFeatures2 features2 = new() { pNext = &features13 };
                _instanceApi.vkGetPhysicalDeviceFeatures2(pd, &features2);

                if (features13.dynamicRendering && features13.synchronization2)
                {
                    _physicalDevice = pd;
                    _graphicsFamily = gf;
                    _presentFamily = pf;
                    break;
                }
            }
        }

        if (_graphicsFamily == uint.MaxValue)
            throw new InvalidOperationException("No suitable Vulkan physical device found");

        HashSet<uint> uniqueFamilies = [_graphicsFamily, _presentFamily];
        VkDeviceQueueCreateInfo* queueInfos = stackalloc VkDeviceQueueCreateInfo[2];
        uint queueInfoCount = 0;
        float priority = 1.0f;
        foreach (uint family in uniqueFamilies)
        {
            queueInfos[queueInfoCount++] = new VkDeviceQueueCreateInfo
            {
                queueFamilyIndex = family,
                queueCount = 1,
                pQueuePriorities = &priority
            };
        }

        VkPhysicalDeviceVulkan13Features enable13 = new()
        {
            dynamicRendering = true,
            synchronization2 = true
        };
        VkPhysicalDeviceFeatures2 enableFeatures = new() { pNext = &enable13 };

        List<VkUtf8String> deviceExtensions = [VK_KHR_SWAPCHAIN_EXTENSION_NAME];
        using VkStringArray vkDeviceExtensions = new(deviceExtensions);

        VkDeviceCreateInfo deviceCreateInfo = new()
        {
            pNext = &enableFeatures,
            queueCreateInfoCount = queueInfoCount,
            pQueueCreateInfos = queueInfos,
            enabledExtensionCount = vkDeviceExtensions.Length,
            ppEnabledExtensionNames = vkDeviceExtensions
        };

        _instanceApi.vkCreateDevice(_physicalDevice, &deviceCreateInfo, null, out _device).CheckResult();
        _api = GetApi(_instance, _device);

        _api.vkGetDeviceQueue(_graphicsFamily, 0, out _graphicsQueue);
        _api.vkGetDeviceQueue(_presentFamily, 0, out _presentQueue);

        CreateSwapchain();

        using var compiler = new Compiler();
        CompileResult vsResult = compiler.Compile(VertexShaderSource, "nuklear.vert", new CompilerOptions
        {
            ShaderStage = ShaderKind.VertexShader,
            TargetEnv = TargetEnvironmentVersion.Vulkan_1_3
        });
        if (vsResult.Status != CompilationStatus.Success)
            throw new InvalidOperationException($"Vertex shader compilation failed: {vsResult.ErrorMessage}");

        CompileResult fsResult = compiler.Compile(FragmentShaderSource, "nuklear.frag", new CompilerOptions
        {
            ShaderStage = ShaderKind.FragmentShader,
            TargetEnv = TargetEnvironmentVersion.Vulkan_1_3
        });
        if (fsResult.Status != CompilationStatus.Success)
            throw new InvalidOperationException($"Fragment shader compilation failed: {fsResult.ErrorMessage}");

        _api.vkCreateShaderModule(vsResult.Bytecode.AsSpan(), null, out VkShaderModule vertModule).CheckResult();
        _api.vkCreateShaderModule(fsResult.Bytecode.AsSpan(), null, out VkShaderModule fragModule).CheckResult();

        VkDescriptorSetLayoutBinding binding = new()
        {
            binding = 0,
            descriptorType = VkDescriptorType.CombinedImageSampler,
            descriptorCount = 1,
            stageFlags = VkShaderStageFlags.Fragment
        };
        VkDescriptorSetLayoutCreateInfo dsLayoutInfo = new()
        {
            bindingCount = 1,
            pBindings = &binding
        };
        _api.vkCreateDescriptorSetLayout(&dsLayoutInfo, null, out _descriptorSetLayout).CheckResult();

        VkPushConstantRange pushRange = new()
        {
            stageFlags = VkShaderStageFlags.Vertex,
            offset = 0,
            size = 64
        };
        VkDescriptorSetLayout dsLayout = _descriptorSetLayout;
        VkPipelineLayoutCreateInfo layoutInfo = new()
        {
            setLayoutCount = 1,
            pSetLayouts = &dsLayout,
            pushConstantRangeCount = 1,
            pPushConstantRanges = &pushRange
        };
        _api.vkCreatePipelineLayout(&layoutInfo, null, out _pipelineLayout).CheckResult();

        VkUtf8ReadOnlyString entryPoint = "main"u8;

        VkPipelineShaderStageCreateInfo* stages = stackalloc VkPipelineShaderStageCreateInfo[2];
        stages[0] = new() { stage = VkShaderStageFlags.Vertex, module = vertModule, pName = entryPoint };
        stages[1] = new() { stage = VkShaderStageFlags.Fragment, module = fragModule, pName = entryPoint };

        VkVertexInputBindingDescription vtxBinding = new()
        {
            binding = 0,
            stride = (uint)sizeof(NkVertex),
            inputRate = VkVertexInputRate.Vertex
        };

        VkVertexInputAttributeDescription* vtxAttrs = stackalloc VkVertexInputAttributeDescription[3];
        vtxAttrs[0] = new() { location = 0, binding = 0, format = VkFormat.R32G32Sfloat, offset = 0 };
        vtxAttrs[1] = new() { location = 1, binding = 0, format = VkFormat.R32G32Sfloat, offset = 8 };
        vtxAttrs[2] = new() { location = 2, binding = 0, format = VkFormat.R8G8B8A8Unorm, offset = 16 };

        VkPipelineVertexInputStateCreateInfo vertexInput = new()
        {
            vertexBindingDescriptionCount = 1,
            pVertexBindingDescriptions = &vtxBinding,
            vertexAttributeDescriptionCount = 3,
            pVertexAttributeDescriptions = vtxAttrs
        };

        VkPipelineInputAssemblyStateCreateInfo inputAssembly = new(VkPrimitiveTopology.TriangleList);
        VkPipelineViewportStateCreateInfo viewportState = new(1, 1);
        VkPipelineMultisampleStateCreateInfo multisample = VkPipelineMultisampleStateCreateInfo.Default;
        VkPipelineDepthStencilStateCreateInfo depthStencil = VkPipelineDepthStencilStateCreateInfo.Default;

        VkPipelineRasterizationStateCreateInfo rasterizer = new()
        {
            polygonMode = VkPolygonMode.Fill,
            cullMode = VkCullModeFlags.None,
            frontFace = VkFrontFace.CounterClockwise,
            lineWidth = 1.0f
        };

        VkPipelineColorBlendAttachmentState blendAttachment = new()
        {
            blendEnable = true,
            srcColorBlendFactor = VkBlendFactor.SrcAlpha,
            dstColorBlendFactor = VkBlendFactor.OneMinusSrcAlpha,
            colorBlendOp = VkBlendOp.Add,
            srcAlphaBlendFactor = VkBlendFactor.One,
            dstAlphaBlendFactor = VkBlendFactor.Zero,
            alphaBlendOp = VkBlendOp.Add,
            colorWriteMask = VkColorComponentFlags.All
        };
        VkPipelineColorBlendStateCreateInfo colorBlend = new()
        {
            attachmentCount = 1,
            pAttachments = &blendAttachment
        };

        VkDynamicState* dynStates = stackalloc VkDynamicState[2];
        dynStates[0] = VkDynamicState.Viewport;
        dynStates[1] = VkDynamicState.Scissor;
        VkPipelineDynamicStateCreateInfo dynamicState = new()
        {
            dynamicStateCount = 2,
            pDynamicStates = dynStates
        };

        VkFormat colorFormat = _swapFormat;
        VkPipelineRenderingCreateInfo renderingInfo = new()
        {
            colorAttachmentCount = 1,
            pColorAttachmentFormats = &colorFormat
        };

        VkGraphicsPipelineCreateInfo pipelineInfo = new()
        {
            pNext = &renderingInfo,
            stageCount = 2,
            pStages = stages,
            pVertexInputState = &vertexInput,
            pInputAssemblyState = &inputAssembly,
            pViewportState = &viewportState,
            pRasterizationState = &rasterizer,
            pMultisampleState = &multisample,
            pDepthStencilState = &depthStencil,
            pColorBlendState = &colorBlend,
            pDynamicState = &dynamicState,
            layout = _pipelineLayout
        };

        _api.vkCreateGraphicsPipeline(pipelineInfo, out _pipeline).CheckResult();

        _api.vkDestroyShaderModule(vertModule);
        _api.vkDestroyShaderModule(fragModule);

        VkSamplerCreateInfo samplerInfo = new()
        {
            magFilter = VkFilter.Linear,
            minFilter = VkFilter.Linear,
            addressModeU = VkSamplerAddressMode.ClampToEdge,
            addressModeV = VkSamplerAddressMode.ClampToEdge,
            addressModeW = VkSamplerAddressMode.ClampToEdge
        };
        _api.vkCreateSampler(&samplerInfo, null, out _sampler).CheckResult();

        VkDescriptorPoolSize poolSize = new()
        {
            type = VkDescriptorType.CombinedImageSampler,
            descriptorCount = 256
        };
        VkDescriptorPoolCreateInfo poolInfo = new()
        {
            flags = VkDescriptorPoolCreateFlags.FreeDescriptorSet,
            maxSets = 256,
            poolSizeCount = 1,
            pPoolSizes = &poolSize
        };
        _api.vkCreateDescriptorPool(&poolInfo, null, out _descriptorPool).CheckResult();

        _commandBuffers = new VkCommandBuffer[_swapImageCount];
        _fences = new VkFence[_swapImageCount];
        _acquireSemaphores = new VkSemaphore[_swapImageCount];
        _renderSemaphores = new VkSemaphore[_swapImageCount];

        _api.vkCreateCommandPool(VkCommandPoolCreateFlags.Transient, _graphicsFamily, out _commandPool).CheckResult();

        for (int i = 0; i < _swapImageCount; i++)
        {
            _api.vkAllocateCommandBuffer(_commandPool, out _commandBuffers[i]).CheckResult();
            _api.vkCreateFence(VkFenceCreateFlags.Signaled, out _fences[i]).CheckResult();
            _api.vkCreateSemaphore(out _acquireSemaphores[i]).CheckResult();
            _api.vkCreateSemaphore(out _renderSemaphores[i]).CheckResult();
        }
    }

    void CreateSwapchain()
    {
        _instanceApi.vkGetPhysicalDeviceSurfaceCapabilitiesKHR(_physicalDevice, _surface, out VkSurfaceCapabilitiesKHR caps).CheckResult();

        uint formatCount = 0;
        _instanceApi.vkGetPhysicalDeviceSurfaceFormatsKHR(_physicalDevice, _surface, &formatCount, null).CheckResult();
        VkSurfaceFormatKHR* formats = stackalloc VkSurfaceFormatKHR[(int)formatCount];
        _instanceApi.vkGetPhysicalDeviceSurfaceFormatsKHR(_physicalDevice, _surface, &formatCount, formats).CheckResult();

        _swapFormat = VkFormat.B8G8R8A8Unorm;
        VkColorSpaceKHR colorSpace = formats[0].colorSpace;
        for (int i = 0; i < formatCount; i++)
        {
            if (formats[i].format == VkFormat.B8G8R8A8Unorm)
            {
                colorSpace = formats[i].colorSpace;
                break;
            }
        }

        uint imageCount = Math.Max(caps.minImageCount, 2);
        if (caps.maxImageCount > 0 && imageCount > caps.maxImageCount)
            imageCount = caps.maxImageCount;

        VkExtent2D extent;
        if (caps.currentExtent.width != uint.MaxValue)
        {
            extent = caps.currentExtent;
        }
        else
        {
            extent = new VkExtent2D(
                Math.Clamp((uint)_width, caps.minImageExtent.width, caps.maxImageExtent.width),
                Math.Clamp((uint)_height, caps.minImageExtent.height, caps.maxImageExtent.height));
        }

        VkSwapchainCreateInfoKHR swapInfo = new()
        {
            surface = _surface,
            minImageCount = imageCount,
            imageFormat = _swapFormat,
            imageColorSpace = colorSpace,
            imageExtent = extent,
            imageArrayLayers = 1,
            imageUsage = VkImageUsageFlags.ColorAttachment,
            imageSharingMode = VkSharingMode.Exclusive,
            preTransform = caps.currentTransform,
            compositeAlpha = VkCompositeAlphaFlagsKHR.Opaque,
            presentMode = VkPresentModeKHR.Fifo,
            clipped = true
        };

        if (_graphicsFamily != _presentFamily)
        {
            uint* families = stackalloc uint[2] { _graphicsFamily, _presentFamily };
            swapInfo.imageSharingMode = VkSharingMode.Concurrent;
            swapInfo.queueFamilyIndexCount = 2;
            swapInfo.pQueueFamilyIndices = families;
        }

        _api.vkCreateSwapchainKHR(&swapInfo, null, out _swapchain).CheckResult();

        uint swapImageCount = 0;
        _api.vkGetSwapchainImagesKHR(_swapchain, &swapImageCount, null).CheckResult();
        _swapImageCount = swapImageCount;
        _swapImages = new VkImage[_swapImageCount];
        fixed (VkImage* pImages = _swapImages)
        {
            _api.vkGetSwapchainImagesKHR(_swapchain, &swapImageCount, pImages).CheckResult();
        }

        _swapImageViews = new VkImageView[_swapImageCount];
        for (int i = 0; i < _swapImageCount; i++)
        {
            VkImageViewCreateInfo viewInfo = new()
            {
                image = _swapImages[i],
                viewType = VkImageViewType.Image2D,
                format = _swapFormat,
                subresourceRange = new VkImageSubresourceRange(VkImageAspectFlags.Color, 0, 1, 0, 1)
            };
            _api.vkCreateImageView(&viewInfo, null, out _swapImageViews[i]).CheckResult();
        }
    }

    void DestroySwapchain()
    {
        for (int i = 0; i < _swapImageViews.Length; i++)
            _api.vkDestroyImageView(_swapImageViews[i]);
        _api.vkDestroySwapchainKHR(_swapchain);
    }

    public void Resize(int w, int h)
    {
        if (w <= 0 || h <= 0) return;
        _width = w;
        _height = h;
        _api.vkDeviceWaitIdle();
        DestroySwapchain();
        CreateSwapchain();
    }

    uint GetMemoryTypeIndex(uint typeBits, VkMemoryPropertyFlags properties)
    {
        _instanceApi.vkGetPhysicalDeviceMemoryProperties(_physicalDevice, out VkPhysicalDeviceMemoryProperties memProps);
        for (int i = 0; i < memProps.memoryTypeCount; i++)
        {
            if ((typeBits & (1u << i)) != 0 && (memProps.memoryTypes[i].propertyFlags & properties) == properties)
                return (uint)i;
        }
        throw new InvalidOperationException("No suitable memory type found");
    }

    void TransitionImage(VkCommandBuffer cmd, VkImage image,
        VkImageLayout oldLayout, VkImageLayout newLayout,
        VkAccessFlags2 srcAccess, VkAccessFlags2 dstAccess,
        VkPipelineStageFlags2 srcStage, VkPipelineStageFlags2 dstStage)
    {
        VkImageMemoryBarrier2 barrier = new()
        {
            srcStageMask = srcStage,
            srcAccessMask = srcAccess,
            dstStageMask = dstStage,
            dstAccessMask = dstAccess,
            oldLayout = oldLayout,
            newLayout = newLayout,
            srcQueueFamilyIndex = VK_QUEUE_FAMILY_IGNORED,
            dstQueueFamilyIndex = VK_QUEUE_FAMILY_IGNORED,
            image = image,
            subresourceRange = new VkImageSubresourceRange(VkImageAspectFlags.Color, 0, 1, 0, 1)
        };
        VkDependencyInfo depInfo = new()
        {
            imageMemoryBarrierCount = 1,
            pImageMemoryBarriers = &barrier
        };
        _api.vkCmdPipelineBarrier2(cmd, &depInfo);
    }

    VkCommandBuffer BeginOneTimeCommands()
    {
        _api.vkAllocateCommandBuffer(_commandPool, out VkCommandBuffer cmd).CheckResult();
        _api.vkBeginCommandBuffer(cmd, VkCommandBufferUsageFlags.OneTimeSubmit).CheckResult();
        return cmd;
    }

    void EndOneTimeCommands(VkCommandBuffer cmd)
    {
        _api.vkEndCommandBuffer(cmd).CheckResult();

        VkSubmitInfo submitInfo = new()
        {
            commandBufferCount = 1,
            pCommandBuffers = &cmd
        };
        _api.vkQueueSubmit(_graphicsQueue, 1, &submitInfo, VkFence.Null).CheckResult();
        _api.vkQueueWaitIdle(_graphicsQueue);
        _api.vkFreeCommandBuffers(_commandPool, cmd);
    }

    public override VulkanTexture CreateTexture(int W, int H, IntPtr Data)
    {
        uint imageSize = (uint)(W * H * 4);

        VkBufferCreateInfo stagingBufInfo = new()
        {
            size = imageSize,
            usage = VkBufferUsageFlags.TransferSrc,
            sharingMode = VkSharingMode.Exclusive
        };
        _api.vkCreateBuffer(&stagingBufInfo, null, out VkBuffer stagingBuffer).CheckResult();
        _api.vkGetBufferMemoryRequirements(stagingBuffer, out VkMemoryRequirements stagingReqs);
        VkMemoryAllocateInfo stagingAlloc = new()
        {
            allocationSize = stagingReqs.size,
            memoryTypeIndex = GetMemoryTypeIndex(stagingReqs.memoryTypeBits, VkMemoryPropertyFlags.HostVisible | VkMemoryPropertyFlags.HostCoherent)
        };
        _api.vkAllocateMemory(&stagingAlloc, null, out VkDeviceMemory stagingMem).CheckResult();
        _api.vkBindBufferMemory(stagingBuffer, stagingMem, 0).CheckResult();

        void* mapped;
        _api.vkMapMemory(stagingMem, 0, imageSize, 0, &mapped).CheckResult();
        Buffer.MemoryCopy((void*)Data, mapped, imageSize, imageSize);
        _api.vkUnmapMemory(stagingMem);

        VkImageCreateInfo imageInfo = new()
        {
            imageType = VkImageType.Image2D,
            format = VkFormat.R8G8B8A8Unorm,
            extent = new VkExtent3D((uint)W, (uint)H, 1),
            mipLevels = 1,
            arrayLayers = 1,
            samples = VkSampleCountFlags.Count1,
            tiling = VkImageTiling.Optimal,
            usage = VkImageUsageFlags.TransferDst | VkImageUsageFlags.Sampled,
            initialLayout = VkImageLayout.Undefined
        };
        _api.vkCreateImage(&imageInfo, null, out VkImage image).CheckResult();
        _api.vkGetImageMemoryRequirements(image, out VkMemoryRequirements imgReqs);
        VkMemoryAllocateInfo imgAlloc = new()
        {
            allocationSize = imgReqs.size,
            memoryTypeIndex = GetMemoryTypeIndex(imgReqs.memoryTypeBits, VkMemoryPropertyFlags.DeviceLocal)
        };
        _api.vkAllocateMemory(&imgAlloc, null, out VkDeviceMemory imageMem).CheckResult();
        _api.vkBindImageMemory(image, imageMem, 0).CheckResult();

        VkCommandBuffer uploadCmd = BeginOneTimeCommands();

        TransitionImage(uploadCmd, image,
            VkImageLayout.Undefined, VkImageLayout.TransferDstOptimal,
            0, VK_ACCESS_2_TRANSFER_WRITE_BIT,
            VK_PIPELINE_STAGE_2_TOP_OF_PIPE_BIT, VK_PIPELINE_STAGE_2_TRANSFER_BIT);

        VkBufferImageCopy copyRegion = new()
        {
            imageSubresource = new VkImageSubresourceLayers(VkImageAspectFlags.Color, 0, 0, 1),
            imageExtent = new VkExtent3D((uint)W, (uint)H, 1)
        };
        _api.vkCmdCopyBufferToImage(uploadCmd, stagingBuffer, image, VkImageLayout.TransferDstOptimal, 1, &copyRegion);

        TransitionImage(uploadCmd, image,
            VkImageLayout.TransferDstOptimal, VkImageLayout.ShaderReadOnlyOptimal,
            VK_ACCESS_2_TRANSFER_WRITE_BIT, VK_ACCESS_2_SHADER_READ_BIT,
            VK_PIPELINE_STAGE_2_TRANSFER_BIT, VK_PIPELINE_STAGE_2_FRAGMENT_SHADER_BIT);

        EndOneTimeCommands(uploadCmd);

        _api.vkDestroyBuffer(stagingBuffer);
        _api.vkFreeMemory(stagingMem);

        VkImageViewCreateInfo viewInfo = new()
        {
            image = image,
            viewType = VkImageViewType.Image2D,
            format = VkFormat.R8G8B8A8Unorm,
            subresourceRange = new VkImageSubresourceRange(VkImageAspectFlags.Color, 0, 1, 0, 1)
        };
        _api.vkCreateImageView(&viewInfo, null, out VkImageView view).CheckResult();

        VkDescriptorSetLayout layout = _descriptorSetLayout;
        VkDescriptorSetAllocateInfo dsAlloc = new()
        {
            descriptorPool = _descriptorPool,
            descriptorSetCount = 1,
            pSetLayouts = &layout
        };
        VkDescriptorSet descriptorSet;
        _api.vkAllocateDescriptorSets(&dsAlloc, &descriptorSet).CheckResult();

        VkDescriptorImageInfo imgDescInfo = new()
        {
            sampler = _sampler,
            imageView = view,
            imageLayout = VkImageLayout.ShaderReadOnlyOptimal
        };
        VkWriteDescriptorSet writeDs = new()
        {
            dstSet = descriptorSet,
            dstBinding = 0,
            descriptorCount = 1,
            descriptorType = VkDescriptorType.CombinedImageSampler,
            pImageInfo = &imgDescInfo
        };
        _api.vkUpdateDescriptorSets(writeDs);

        return new VulkanTexture(image, imageMem, view, descriptorSet);
    }

    public override void SetBuffer(ReadOnlySpan<NkVertex> VertexBuffer, ReadOnlySpan<ushort> IndexBuffer)
    {
        uint vertBytes = (uint)(VertexBuffer.Length * sizeof(NkVertex));
        uint indBytes = (uint)(IndexBuffer.Length * sizeof(ushort));

        EnsureBuffer(ref _vertexBuffer, ref _vertexMemory, ref _vertexBufferSize, vertBytes, VkBufferUsageFlags.VertexBuffer);
        EnsureBuffer(ref _indexBuffer, ref _indexMemory, ref _indexBufferSize, indBytes, VkBufferUsageFlags.IndexBuffer);

        void* mapped;
        _api.vkMapMemory(_vertexMemory, 0, vertBytes, 0, &mapped).CheckResult();
        fixed (NkVertex* src = VertexBuffer)
            Buffer.MemoryCopy(src, mapped, vertBytes, vertBytes);
        _api.vkUnmapMemory(_vertexMemory);

        _api.vkMapMemory(_indexMemory, 0, indBytes, 0, &mapped).CheckResult();
        fixed (ushort* src = IndexBuffer)
            Buffer.MemoryCopy(src, mapped, indBytes, indBytes);
        _api.vkUnmapMemory(_indexMemory);
    }

    void EnsureBuffer(ref VkBuffer buffer, ref VkDeviceMemory memory, ref uint currentSize, uint requiredSize, VkBufferUsageFlags usage)
    {
        if (currentSize >= requiredSize && buffer != VkBuffer.Null)
            return;

        if (buffer != VkBuffer.Null)
        {
            _api.vkDestroyBuffer(buffer);
            _api.vkFreeMemory(memory);
        }

        currentSize = requiredSize;
        VkBufferCreateInfo bufInfo = new()
        {
            size = requiredSize,
            usage = usage,
            sharingMode = VkSharingMode.Exclusive
        };
        _api.vkCreateBuffer(&bufInfo, null, out buffer).CheckResult();
        _api.vkGetBufferMemoryRequirements(buffer, out VkMemoryRequirements reqs);
        VkMemoryAllocateInfo allocInfo = new()
        {
            allocationSize = reqs.size,
            memoryTypeIndex = GetMemoryTypeIndex(reqs.memoryTypeBits, VkMemoryPropertyFlags.HostVisible | VkMemoryPropertyFlags.HostCoherent)
        };
        _api.vkAllocateMemory(&allocInfo, null, out memory).CheckResult();
        _api.vkBindBufferMemory(buffer, memory, 0).CheckResult();
    }

    public void BeginRender()
    {
        VkFence fence = _fences[_frameIndex];
        _api.vkWaitForFences(1, &fence, true, ulong.MaxValue).CheckResult();
        _api.vkResetFences(1, &fence).CheckResult();

        uint imageIndex;
        VkResult acquireResult = _api.vkAcquireNextImageKHR(_swapchain, ulong.MaxValue, _acquireSemaphores[_frameIndex], VkFence.Null, &imageIndex);
        if (acquireResult == VkResult.ErrorOutOfDateKHR)
        {
            Resize(_width, _height);
            return;
        }

        _frameIndex = imageIndex;

        _api.vkResetCommandPool(_commandPool, VkCommandPoolResetFlags.None);
        _cmd = _commandBuffers[_frameIndex];
        _api.vkBeginCommandBuffer(_cmd, VkCommandBufferUsageFlags.OneTimeSubmit).CheckResult();

        TransitionImage(_cmd, _swapImages[_frameIndex],
            VkImageLayout.Undefined, VkImageLayout.ColorAttachmentOptimal,
            0, VK_ACCESS_2_COLOR_ATTACHMENT_WRITE_BIT,
            VK_PIPELINE_STAGE_2_TOP_OF_PIPE_BIT, VK_PIPELINE_STAGE_2_COLOR_ATTACHMENT_OUTPUT_BIT);

        VkRenderingAttachmentInfo colorAttachment = new()
        {
            imageView = _swapImageViews[_frameIndex],
            imageLayout = VkImageLayout.ColorAttachmentOptimal,
            loadOp = VkAttachmentLoadOp.Clear,
            storeOp = VkAttachmentStoreOp.Store,
            clearValue = new VkClearValue(0.392f, 0.584f, 0.929f, 1.0f)
        };

        VkRenderingInfo renderInfo = new()
        {
            renderArea = new VkRect2D(0, 0, (uint)_width, (uint)_height),
            layerCount = 1,
            colorAttachmentCount = 1,
            pColorAttachments = &colorAttachment
        };
        _api.vkCmdBeginRendering(_cmd, &renderInfo);

        _api.vkCmdSetViewport(_cmd, 0, 0, _width, _height);
        _api.vkCmdBindPipeline(_cmd, VkPipelineBindPoint.Graphics, _pipeline);

        var proj = Matrix4x4.CreateOrthographicOffCenter(0, _width, 0, _height, -1, 1);
        _api.vkCmdPushConstants(_cmd, _pipelineLayout, VkShaderStageFlags.Vertex, 0, 64, &proj);

        if (_vertexBuffer != VkBuffer.Null)
            _api.vkCmdBindVertexBuffer(_cmd, 0, _vertexBuffer);
        if (_indexBuffer != VkBuffer.Null)
            _api.vkCmdBindIndexBuffer(_cmd, _indexBuffer, 0, VkIndexType.Uint16);
    }

    public override void Render(NkHandle Userdata, VulkanTexture Texture, NkRect ClipRect, uint Offset, uint Count)
    {
        int x = Math.Max(0, (int)ClipRect.X);
        int y = Math.Max(0, (int)ClipRect.Y);
        uint w = (uint)Math.Max(0, (int)ClipRect.W);
        uint h = (uint)Math.Max(0, (int)ClipRect.H);

        if (w == 0 || h == 0) return;

        _api.vkCmdSetScissor(_cmd, x, y, w, h);
        _api.vkCmdBindDescriptorSets(_cmd, VkPipelineBindPoint.Graphics, _pipelineLayout, 0, Texture.DescriptorSet);
        _api.vkCmdDrawIndexed(_cmd, Count, 1, Offset, 0, 0);
    }

    public void EndRender()
    {
        _api.vkCmdEndRendering(_cmd);

        TransitionImage(_cmd, _swapImages[_frameIndex],
            VkImageLayout.ColorAttachmentOptimal, VkImageLayout.PresentSrcKHR,
            VK_ACCESS_2_COLOR_ATTACHMENT_WRITE_BIT, 0,
            VK_PIPELINE_STAGE_2_COLOR_ATTACHMENT_OUTPUT_BIT, VK_PIPELINE_STAGE_2_BOTTOM_OF_PIPE_BIT);

        _api.vkEndCommandBuffer(_cmd).CheckResult();

        VkSemaphore waitSem = _acquireSemaphores[_frameIndex];
        VkSemaphore signalSem = _renderSemaphores[_frameIndex];
        VkPipelineStageFlags waitStage = VkPipelineStageFlags.ColorAttachmentOutput;
        VkCommandBuffer cmd = _cmd;

        VkSubmitInfo submitInfo = new()
        {
            waitSemaphoreCount = 1,
            pWaitSemaphores = &waitSem,
            pWaitDstStageMask = &waitStage,
            commandBufferCount = 1,
            pCommandBuffers = &cmd,
            signalSemaphoreCount = 1,
            pSignalSemaphores = &signalSem
        };
        _api.vkQueueSubmit(_graphicsQueue, 1, &submitInfo, _fences[_frameIndex]).CheckResult();

        _api.vkQueuePresentKHR(_presentQueue, signalSem, _swapchain, _frameIndex);
    }
}

#region Win32 P/Invoke

static unsafe partial class Win32
{
    public const uint WS_OVERLAPPEDWINDOW = 0x00CF0000;
    public const uint WS_VISIBLE = 0x10000000;
    public const int CW_USEDEFAULT = unchecked((int)0x80000000);
    public const int CS_HREDRAW = 0x0002;
    public const int CS_VREDRAW = 0x0001;
    public const int IDC_ARROW = 32512;

    public const uint WM_DESTROY = 0x0002;
    public const uint WM_SIZE = 0x0005;
    public const uint WM_CLOSE = 0x0010;
    public const uint WM_QUIT = 0x0012;
    public const uint WM_KEYDOWN = 0x0100;
    public const uint WM_KEYUP = 0x0101;
    public const uint WM_CHAR = 0x0102;
    public const uint WM_MOUSEMOVE = 0x0200;
    public const uint WM_LBUTTONDOWN = 0x0201;
    public const uint WM_LBUTTONUP = 0x0202;
    public const uint WM_RBUTTONDOWN = 0x0204;
    public const uint WM_RBUTTONUP = 0x0205;
    public const uint WM_MBUTTONDOWN = 0x0207;
    public const uint WM_MBUTTONUP = 0x0208;
    public const uint WM_MOUSEWHEEL = 0x020A;

    public const int VK_BACK = 0x08;
    public const int VK_TAB = 0x09;
    public const int VK_RETURN = 0x0D;
    public const int VK_SHIFT = 0x10;
    public const int VK_CONTROL = 0x11;
    public const int VK_DELETE = 0x2E;
    public const int VK_LEFT = 0x25;
    public const int VK_UP = 0x26;
    public const int VK_RIGHT = 0x27;
    public const int VK_DOWN = 0x28;
    public const int VK_HOME = 0x24;
    public const int VK_END = 0x23;

    public const uint PM_REMOVE = 0x0001;

    public delegate nint WndProc(nint hWnd, uint msg, nuint wParam, nint lParam);

    [StructLayout(LayoutKind.Sequential, CharSet = CharSet.Unicode)]
    public struct WNDCLASSEXW
    {
        public int cbSize;
        public int style;
        public nint lpfnWndProc;
        public int cbClsExtra;
        public int cbWndExtra;
        public nint hInstance;
        public nint hIcon;
        public nint hCursor;
        public nint hbrBackground;
        public nint lpszMenuName;
        public char* lpszClassName;
        public nint hIconSm;
    }

    [StructLayout(LayoutKind.Sequential)]
    public struct MSG
    {
        public nint hwnd;
        public uint message;
        public nuint wParam;
        public nint lParam;
        public uint time;
        public int pt_x;
        public int pt_y;
    }

    [StructLayout(LayoutKind.Sequential)]
    public struct RECT
    {
        public int Left, Top, Right, Bottom;
    }

    [LibraryImport("user32.dll", SetLastError = true)]
    public static partial ushort RegisterClassExW(WNDCLASSEXW* wc);

    [LibraryImport("user32.dll", StringMarshalling = StringMarshalling.Utf16, SetLastError = true)]
    public static partial nint CreateWindowExW(
        uint dwExStyle, char* lpClassName, string lpWindowName, uint dwStyle,
        int x, int y, int nWidth, int nHeight,
        nint hWndParent, nint hMenu, nint hInstance, nint lpParam);

    [LibraryImport("user32.dll")]
    [return: MarshalAs(UnmanagedType.Bool)]
    public static partial bool ShowWindow(nint hWnd, int nCmdShow);

    [LibraryImport("user32.dll")]
    [return: MarshalAs(UnmanagedType.Bool)]
    public static partial bool PeekMessageW(MSG* lpMsg, nint hWnd, uint wMsgFilterMin, uint wMsgFilterMax, uint wRemoveMsg);

    [LibraryImport("user32.dll")]
    [return: MarshalAs(UnmanagedType.Bool)]
    public static partial bool TranslateMessage(MSG* lpMsg);

    [LibraryImport("user32.dll")]
    public static partial nint DispatchMessageW(MSG* lpMsg);

    [LibraryImport("user32.dll")]
    public static partial nint DefWindowProcW(nint hWnd, uint msg, nuint wParam, nint lParam);

    [LibraryImport("user32.dll")]
    public static partial void PostQuitMessage(int nExitCode);

    [LibraryImport("kernel32.dll", StringMarshalling = StringMarshalling.Utf16)]
    public static partial nint GetModuleHandleW(string? lpModuleName);

    [LibraryImport("user32.dll")]
    public static partial nint LoadCursorW(nint hInstance, nint lpCursorName);

    [LibraryImport("user32.dll")]
    [return: MarshalAs(UnmanagedType.Bool)]
    public static partial bool GetClientRect(nint hWnd, RECT* lpRect);

    [LibraryImport("user32.dll")]
    public static partial short GetKeyState(int nVirtKey);

    [LibraryImport("user32.dll")]
    public static partial nint SetCapture(nint hWnd);

    [LibraryImport("user32.dll")]
    [return: MarshalAs(UnmanagedType.Bool)]
    public static partial bool ReleaseCapture();

    public static bool IsKeyDown(int vk) => (GetKeyState(vk) & 0x8000) != 0;
    public static int GET_X_LPARAM(nint lp) => (short)(lp & 0xFFFF);
    public static int GET_Y_LPARAM(nint lp) => (short)((lp >> 16) & 0xFFFF);
    public static short GET_WHEEL_DELTA(nuint wp) => (short)((wp >> 16) & 0xFFFF);
}

#endregion

static unsafe class Program
{
    static VulkanDevice? s_dev;
    static nint s_hwnd;
    static bool s_running = true;

    [STAThread]
    static void Main()
    {
        nint hInstance = Win32.GetModuleHandleW(null);

        fixed (char* className = "NuklearVulkan")
        {
            var wc = new Win32.WNDCLASSEXW
            {
                cbSize = sizeof(Win32.WNDCLASSEXW),
                style = Win32.CS_HREDRAW | Win32.CS_VREDRAW,
                lpfnWndProc = Marshal.GetFunctionPointerForDelegate(s_wndProcDelegate),
                hInstance = hInstance,
                hCursor = Win32.LoadCursorW(0, Win32.IDC_ARROW),
                lpszClassName = className,
            };
            Win32.RegisterClassExW(&wc);

            s_hwnd = Win32.CreateWindowExW(
                0, className, "NuklearDotNet - Vulkan Example",
                Win32.WS_OVERLAPPEDWINDOW | Win32.WS_VISIBLE,
                Win32.CW_USEDEFAULT, Win32.CW_USEDEFAULT, 1280, 800,
                0, 0, hInstance, 0);
        }

        Win32.RECT clientRect;
        Win32.GetClientRect(s_hwnd, &clientRect);
        int w = clientRect.Right - clientRect.Left;
        int h = clientRect.Bottom - clientRect.Top;

        s_dev = new VulkanDevice(s_hwnd, hInstance, w, h);
        Shared.Init(s_dev);

        var sw = Stopwatch.StartNew();
        float dt = 0.016f;

        while (s_running)
        {
            Win32.MSG msg;
            while (Win32.PeekMessageW(&msg, 0, 0, 0, Win32.PM_REMOVE))
            {
                if (msg.message == Win32.WM_QUIT)
                {
                    s_running = false;
                    break;
                }
                Win32.TranslateMessage(&msg);
                Win32.DispatchMessageW(&msg);
            }

            if (!s_running) break;

            Shared.DrawLoop(dt);

            dt = Math.Max((float)sw.Elapsed.TotalSeconds, 0.001f);
            sw.Restart();
        }
    }

    static readonly Win32.WndProc s_wndProcDelegate = WndProc;

    static nint WndProc(nint hWnd, uint msg, nuint wParam, nint lParam)
    {
        switch (msg)
        {
            case Win32.WM_DESTROY:
                Win32.PostQuitMessage(0);
                return 0;

            case Win32.WM_SIZE:
            {
                int w = Win32.GET_X_LPARAM(lParam);
                int h = Win32.GET_Y_LPARAM(lParam);
                if (w > 0 && h > 0)
                    s_dev?.Resize(w, h);
                return 0;
            }

            case Win32.WM_MOUSEMOVE:
                s_dev?.OnMouseMove(Win32.GET_X_LPARAM(lParam), Win32.GET_Y_LPARAM(lParam));
                return 0;

            case Win32.WM_LBUTTONDOWN:
                s_dev?.OnMouseButton(NuklearEvent.MouseButton.Left, Win32.GET_X_LPARAM(lParam), Win32.GET_Y_LPARAM(lParam), true);
                Win32.SetCapture(hWnd);
                return 0;
            case Win32.WM_LBUTTONUP:
                s_dev?.OnMouseButton(NuklearEvent.MouseButton.Left, Win32.GET_X_LPARAM(lParam), Win32.GET_Y_LPARAM(lParam), false);
                Win32.ReleaseCapture();
                return 0;

            case Win32.WM_RBUTTONDOWN:
                s_dev?.OnMouseButton(NuklearEvent.MouseButton.Right, Win32.GET_X_LPARAM(lParam), Win32.GET_Y_LPARAM(lParam), true);
                Win32.SetCapture(hWnd);
                return 0;
            case Win32.WM_RBUTTONUP:
                s_dev?.OnMouseButton(NuklearEvent.MouseButton.Right, Win32.GET_X_LPARAM(lParam), Win32.GET_Y_LPARAM(lParam), false);
                Win32.ReleaseCapture();
                return 0;

            case Win32.WM_MBUTTONDOWN:
                s_dev?.OnMouseButton(NuklearEvent.MouseButton.Middle, Win32.GET_X_LPARAM(lParam), Win32.GET_Y_LPARAM(lParam), true);
                Win32.SetCapture(hWnd);
                return 0;
            case Win32.WM_MBUTTONUP:
                s_dev?.OnMouseButton(NuklearEvent.MouseButton.Middle, Win32.GET_X_LPARAM(lParam), Win32.GET_Y_LPARAM(lParam), false);
                Win32.ReleaseCapture();
                return 0;

            case Win32.WM_MOUSEWHEEL:
                s_dev?.OnScroll(0, Win32.GET_WHEEL_DELTA(wParam) / 120f);
                return 0;

            case Win32.WM_CHAR:
            {
                char c = (char)wParam;
                if (c >= 32)
                    s_dev?.OnText(c.ToString());
                return 0;
            }

            case Win32.WM_KEYDOWN:
            case Win32.WM_KEYUP:
            {
                bool down = msg == Win32.WM_KEYDOWN;
                int vk = (int)wParam;
                bool ctrl = Win32.IsKeyDown(Win32.VK_CONTROL);

                switch (vk)
                {
                    case Win32.VK_SHIFT: s_dev?.OnKey(NkKeys.Shift, down); break;
                    case Win32.VK_CONTROL: s_dev?.OnKey(NkKeys.Ctrl, down); break;
                    case Win32.VK_DELETE: s_dev?.OnKey(NkKeys.Del, down); break;
                    case Win32.VK_RETURN: s_dev?.OnKey(NkKeys.Enter, down); break;
                    case Win32.VK_TAB: s_dev?.OnKey(NkKeys.Tab, down); break;
                    case Win32.VK_BACK: s_dev?.OnKey(NkKeys.Backspace, down); break;
                    case Win32.VK_UP: s_dev?.OnKey(NkKeys.Up, down); break;
                    case Win32.VK_DOWN: s_dev?.OnKey(NkKeys.Down, down); break;
                    case Win32.VK_HOME: s_dev?.OnKey(NkKeys.LineStart, down); break;
                    case Win32.VK_END: s_dev?.OnKey(NkKeys.LineEnd, down); break;

                    case Win32.VK_LEFT:
                        if (ctrl && down) s_dev?.OnKey(NkKeys.TextWordLeft, true);
                        else s_dev?.OnKey(NkKeys.Left, down);
                        break;
                    case Win32.VK_RIGHT:
                        if (ctrl && down) s_dev?.OnKey(NkKeys.TextWordRight, true);
                        else s_dev?.OnKey(NkKeys.Right, down);
                        break;

                    case 'C' when ctrl && down: s_dev?.OnKey(NkKeys.Copy, true); break;
                    case 'V' when ctrl && down: s_dev?.OnKey(NkKeys.Paste, true); break;
                    case 'X' when ctrl && down: s_dev?.OnKey(NkKeys.Cut, true); break;
                    case 'A' when ctrl && down: s_dev?.OnKey(NkKeys.TextSelectAll, true); break;
                }
                return 0;
            }
        }

        return Win32.DefWindowProcW(hWnd, msg, wParam, lParam);
    }
}
