cmake_minimum_required(VERSION 3.20)
project(Nuklear2 C)

# Static-link the VC runtime so the DLL has zero runtime dependencies
if(MSVC)
  set(CMAKE_MSVC_RUNTIME_LIBRARY "MultiThreaded$<$<CONFIG:Debug>:Debug>")
endif()

# Build both shared and static libraries
add_library(Nuklear2 SHARED nuklear_build.c)
add_library(Nuklear2_static STATIC nuklear_build.c)

# Both targets need the nuklear headers
target_include_directories(Nuklear2 PRIVATE ${CMAKE_CURRENT_SOURCE_DIR}/../nuklear2_c/src)
target_include_directories(Nuklear2_static PRIVATE ${CMAKE_CURRENT_SOURCE_DIR}/../nuklear2_c/src)

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
