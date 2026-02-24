#define NK_INCLUDE_DEFAULT_ALLOCATOR
#define NK_INCLUDE_STANDARD_VARARGS

#include <string.h>
#ifndef NK_MEMCPY
#define NK_MEMCPY memcpy
#endif
#ifndef NK_MEMSET
#define NK_MEMSET memset
#endif
#ifndef NK_ASSERT
#define NK_ASSERT(ex) do { if(!(ex)) { *(int*)0 = 0; } } while(0)
#endif

#include "nuklear.h"
#include <SDL3/SDL.h>

/* SDL3 3.2.x renamed face buttons: A→SOUTH, B→EAST, X→WEST, Y→NORTH.
 * nuklear_gamepad_sdl3.h still uses the old names. */
#ifndef SDL_GAMEPAD_BUTTON_A
#define SDL_GAMEPAD_BUTTON_A     SDL_GAMEPAD_BUTTON_SOUTH
#define SDL_GAMEPAD_BUTTON_B     SDL_GAMEPAD_BUTTON_EAST
#define SDL_GAMEPAD_BUTTON_X     SDL_GAMEPAD_BUTTON_WEST
#define SDL_GAMEPAD_BUTTON_Y     SDL_GAMEPAD_BUTTON_NORTH
#endif

/* SDL3 3.2.x renamed the SDL_Event union field from .gamepad to .gdevice.
 * nuklear_gamepad_sdl3.h accesses event->gamepad.which — redirect to gdevice.
 * This is safe: C macro replacement only matches complete tokens, so function
 * names containing "gamepad" as a substring (SDL_IsGamepad etc.) are unaffected. */
#define gamepad gdevice

#define NK_GAMEPAD_SDL3
#define NK_GAMEPAD_KEYBOARD
#define NK_GAMEPAD_IMPLEMENTATION
#include "../vendor/nuklear_console/vendor/nuklear_gamepad/nuklear_gamepad.h"

#undef gamepad

/* nk_malloc is defined as NK_INTERN (static) inside nuklear.h's NK_IMPLEMENTATION
 * section, which this TU does not include. Without a visible declaration, MSVC's
 * C compiler implicitly declares nk_malloc as returning int, truncating the void*
 * return value to 32 bits on x64 and producing a corrupted pointer. Bypass by
 * routing NK_CONSOLE_MALLOC/FREE directly to stdlib. */
#include <stdlib.h>
#define NK_CONSOLE_MALLOC(handle, old, size) malloc(size)
#define NK_CONSOLE_FREE(handle, ptr) free(ptr)

/* Hold-to-repeat state for d-pad navigation and slider adjustment.
 * Static: only one widget processes input per frame (input_processed guard).
 * Channels: [0]=left, [1]=right, [2]=up, [3]=down */
static float g_nk_console_dt = 0.0f;
static float g_nk_hold_time[4] = {0.0f, 0.0f, 0.0f, 0.0f};

#define NK_HOLD_INITIAL_DELAY 0.35f
#define NK_HOLD_REPEAT_INTERVAL 0.06f

/* Enable dpad repeat in nuklear_console.h for up/down/left/right navigation */
#define NK_CONSOLE_DPAD_REPEAT

#define NK_CONSOLE_IMPLEMENTATION
#include "../vendor/nuklear_console/nuklear_console.h"

#if defined(_WIN32)
  #define NK_EXPORT __declspec(dllexport)
#else
  #define NK_EXPORT __attribute__((visibility("default")))
#endif

NK_EXPORT void nk_console_set_delta_time(float dt) {
    g_nk_console_dt = dt;
}

NK_EXPORT void nk_console_set_visible(nk_console* widget, nk_bool visible) {
    if (widget != NULL) widget->visible = visible;
}

NK_EXPORT void nk_console_set_selectable(nk_console* widget, nk_bool selectable) {
    if (widget != NULL) widget->selectable = selectable;
}

NK_EXPORT struct nk_rect nk_console_get_active_bounds(nk_console* console) {
    if (console == NULL) return nk_rect(0, 0, 0, 0);
    nk_console_top_data* data = (nk_console_top_data*)console->data;
    nk_console* parent = data->active_parent ? data->active_parent : console;
    if (parent->activeWidget != NULL)
        return parent->activeWidget->bounds;
    return nk_rect(0, 0, 0, 0);
}

NK_EXPORT nk_bool nk_sdl3_gamepad_init(void) {
    SDL_SetHint(SDL_HINT_JOYSTICK_ALLOW_BACKGROUND_EVENTS, "1");
    return SDL_Init(SDL_INIT_GAMEPAD) ? nk_true : nk_false;
}

NK_EXPORT void nk_sdl3_gamepad_shutdown(void) {
    SDL_QuitSubSystem(SDL_INIT_GAMEPAD);
}

NK_EXPORT void nk_sdl3_gamepad_pump_events(struct nk_gamepads* gamepads) {
    /* SDL_PollEvent implicitly pumps events and dequeues them.
     * Forward gamepad connect/disconnect to the handler so gamepads
     * get opened/closed and their slots updated. */
    SDL_Event event;
    while (SDL_PollEvent(&event)) {
        switch (event.type) {
            case SDL_EVENT_GAMEPAD_ADDED:
            case SDL_EVENT_GAMEPAD_REMOVED:
                nk_gamepad_sdl3_handle_event(gamepads, &event);
                break;
        }
    }
}

NK_EXPORT int nk_debug_sizeof_gamepads(void) {
    return (int)sizeof(struct nk_gamepads);
}

NK_EXPORT int nk_debug_sizeof_gamepad(void) {
    return (int)sizeof(struct nk_gamepad);
}

NK_EXPORT int nk_debug_sizeof_gamepad_input_source(void) {
    return (int)sizeof(struct nk_gamepad_input_source);
}
