cmake_minimum_required(VERSION 3.20)
project(Nuklear2 C)

# Static-link the VC runtime so the DLL has zero runtime dependencies
if(MSVC)
  set(CMAKE_MSVC_RUNTIME_LIBRARY "MultiThreaded$<$<CONFIG:Debug>:Debug>")
endif()

# SDL3 static lib gets linked into Nuklear2.so — must be built with -fPIC
set(CMAKE_POSITION_INDEPENDENT_CODE ON)

# When built as subdirectory, SDL3 target is already available from parent
if(NOT TARGET SDL3::SDL3-static)
    include(FetchContent)
    FetchContent_Declare(SDL3
        GIT_REPOSITORY https://github.com/libsdl-org/SDL.git
        GIT_TAG        release-3.4.2
        GIT_SHALLOW    TRUE
    )
    set(SDL_SHARED OFF CACHE BOOL "" FORCE)
    set(SDL_STATIC ON CACHE BOOL "" FORCE)
    set(SDL_TEST_LIBRARY OFF CACHE BOOL "" FORCE)
    set(SDL_DISABLE_INSTALL ON CACHE BOOL "" FORCE)
    set(SDL_AUDIO OFF CACHE BOOL "" FORCE)
    set(SDL_VIDEO OFF CACHE BOOL "" FORCE)
    set(SDL_RENDER OFF CACHE BOOL "" FORCE)
    set(SDL_CAMERA OFF CACHE BOOL "" FORCE)
    set(SDL_GPU OFF CACHE BOOL "" FORCE)
    if(NOT WIN32)
        set(SDL_UNIX_CONSOLE_BUILD ON CACHE BOOL "" FORCE)
    endif()
    FetchContent_MakeAvailable(SDL3)
endif()

set(NK_SOURCES nuklear_build.c nuklear_console_build.c)

set(NK_HEADER_DIR ${CMAKE_CURRENT_SOURCE_DIR}/../nuklear2_c/src)
set(NK_CONSOLE_DIR ${CMAKE_CURRENT_SOURCE_DIR}/../vendor/nuklear_console)
set(NK_GAMEPAD_DIR ${NK_CONSOLE_DIR}/vendor/nuklear_gamepad)
set(NK_CVECTOR_DIR ${NK_CONSOLE_DIR}/vendor/c-vector)

set(NK_INCLUDE_DIRS
    ${NK_HEADER_DIR}
    ${NK_CONSOLE_DIR}
    ${NK_GAMEPAD_DIR}
    ${NK_CVECTOR_DIR}
)

add_library(Nuklear2 SHARED ${NK_SOURCES})
add_library(Nuklear2_static STATIC ${NK_SOURCES})

target_include_directories(Nuklear2 PRIVATE ${NK_INCLUDE_DIRS})
target_include_directories(Nuklear2_static PRIVATE ${NK_INCLUDE_DIRS})

target_link_libraries(Nuklear2 PRIVATE SDL3::SDL3-static)
target_link_libraries(Nuklear2_static PRIVATE SDL3::SDL3-static)

# Suppress warnings from vendored nuklear C code
if(MSVC)
    # C5287: different enum types, C4116: unnamed type in parentheses
    target_compile_options(Nuklear2 PRIVATE /wd5287 /wd4116)
    target_compile_options(Nuklear2_static PRIVATE /wd5287 /wd4116)
endif()

if(WIN32)
    set_target_properties(Nuklear2 PROPERTIES
        PREFIX ""
        IMPORT_PREFIX ""
    )
    set_target_properties(Nuklear2_static PROPERTIES
        PREFIX ""
        OUTPUT_NAME "Nuklear2_static"
    )
    target_sources(Nuklear2 PRIVATE nuklear.def)

    target_link_options(Nuklear2 PRIVATE
        /DELAYLOAD:winmm.dll
        /DELAYLOAD:version.dll
    )
    target_link_libraries(Nuklear2 PRIVATE delayimp.lib)
elseif(APPLE)
    set_target_properties(Nuklear2 PROPERTIES PREFIX "lib" SUFFIX ".dylib")
    set_target_properties(Nuklear2_static PROPERTIES PREFIX "lib" OUTPUT_NAME "Nuklear2_static")
    target_compile_options(Nuklear2 PRIVATE -fvisibility=default)
    target_compile_options(Nuklear2_static PRIVATE -fPIC)
else()
    set_target_properties(Nuklear2 PROPERTIES PREFIX "lib" SUFFIX ".so")
    set_target_properties(Nuklear2_static PROPERTIES PREFIX "lib" OUTPUT_NAME "Nuklear2_static")
    target_compile_options(Nuklear2 PRIVATE -fvisibility=default)
    target_compile_options(Nuklear2_static PRIVATE -fPIC)
endif()

install(TARGETS Nuklear2 Nuklear2_static
    RUNTIME DESTINATION .
    LIBRARY DESTINATION .
    ARCHIVE DESTINATION .)
