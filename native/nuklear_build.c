#define NK_IMPLEMENTATION

#define NK_ZERO_COMMAND_MEMORY
#define NK_BUTTON_TRIGGER_ON_RELEASE

#define NK_INCLUDE_FONT_BAKING
#define NK_INCLUDE_DEFAULT_FONT
#define NK_INCLUDE_VERTEX_BUFFER_OUTPUT
#define NK_INCLUDE_COMMAND_USERDATA

#define NK_INCLUDE_DEFAULT_ALLOCATOR
#define NK_INCLUDE_STANDARD_VARARGS

#define NK_INPUT_MAX 512

#include <stdarg.h>
#include <stdlib.h>

/* Crash on assertion failure, it is captured as an exception in .NET */
#define NK_ASSERT(ex) do { if(!(ex)) { *(int*)0 = 0; } } while(0)

#include "nuklear.h"
#include "nuklear_internal.h"

/* Include all implementation files (multi-file build via includes) */
#include "nuklear_math.c"
#include "nuklear_util.c"
#include "nuklear_utf8.c"
#include "nuklear_buffer.c"
#include "nuklear_string.c"
#include "nuklear_draw.c"
#include "nuklear_vertex.c"
#include "nuklear_font.c"
#include "nuklear_input.c"
#include "nuklear_style.c"
#include "nuklear_context.c"
#include "nuklear_pool.c"
#include "nuklear_table.c"
#include "nuklear_page_element.c"
#include "nuklear_panel.c"
#include "nuklear_window.c"
#include "nuklear_popup.c"
#include "nuklear_group.c"
#include "nuklear_list_view.c"
#include "nuklear_tree.c"
#include "nuklear_widget.c"
#include "nuklear_text.c"
#include "nuklear_button.c"
#include "nuklear_toggle.c"
#include "nuklear_selectable.c"
#include "nuklear_slider.c"
#include "nuklear_knob.c"
#include "nuklear_progress.c"
#include "nuklear_scrollbar.c"
#include "nuklear_property.c"
#include "nuklear_color_picker.c"
#include "nuklear_edit.c"
#include "nuklear_text_editor.c"
#include "nuklear_chart.c"
#include "nuklear_layout.c"
#include "nuklear_combo.c"
#include "nuklear_menu.c"
#include "nuklear_contextual.c"
#include "nuklear_tooltip.c"
#include "nuklear_color.c"
#include "nuklear_image.c"
#include "nuklear_9slice.c"

#include <stdio.h>

/* Debug helper functions - export struct sizes for C# comparison */
#if defined(_WIN32)
  #define NK_EXPORT __declspec(dllexport)
#else
  #define NK_EXPORT __attribute__((visibility("default")))
#endif

NK_EXPORT int nk_debug_sizeof_context(void) { return (int)sizeof(struct nk_context); }
NK_EXPORT int nk_debug_sizeof_buffer(void) { return (int)sizeof(struct nk_buffer); }
NK_EXPORT int nk_debug_sizeof_convert_config(void) { return (int)sizeof(struct nk_convert_config); }
NK_EXPORT int nk_debug_sizeof_draw_null_texture(void) { return (int)sizeof(struct nk_draw_null_texture); }
NK_EXPORT int nk_debug_sizeof_allocator(void) { return (int)sizeof(struct nk_allocator); }
NK_EXPORT int nk_debug_sizeof_handle(void) { return (int)sizeof(nk_handle); }
NK_EXPORT int nk_debug_sizeof_draw_list(void) { return (int)sizeof(struct nk_draw_list); }
NK_EXPORT int nk_debug_sizeof_style(void) { return (int)sizeof(struct nk_style); }
NK_EXPORT int nk_debug_sizeof_input(void) { return (int)sizeof(struct nk_input); }
NK_EXPORT int nk_debug_sizeof_font_atlas(void) { return (int)sizeof(struct nk_font_atlas); }

NK_EXPORT int nk_debug_sizeof_draw_command(void) { return (int)sizeof(struct nk_draw_command); }
NK_EXPORT int nk_debug_sizeof_draw_vertex_layout_element(void) { return (int)sizeof(struct nk_draw_vertex_layout_element); }
NK_EXPORT int nk_debug_sizeof_cursor(void) { return (int)sizeof(struct nk_cursor); }
NK_EXPORT int nk_debug_sizeof_image(void) { return (int)sizeof(struct nk_image); }
NK_EXPORT int nk_debug_sizeof_font(void) { return (int)sizeof(struct nk_font); }
NK_EXPORT int nk_debug_sizeof_user_font(void) { return (int)sizeof(struct nk_user_font); }
NK_EXPORT int nk_debug_sizeof_baked_font(void) { return (int)sizeof(struct nk_baked_font); }
NK_EXPORT int nk_debug_sizeof_font_glyph(void) { return (int)sizeof(struct nk_font_glyph); }
NK_EXPORT int nk_debug_sizeof_font_config(void) { return (int)sizeof(struct nk_font_config); }
NK_EXPORT int nk_debug_offset_atlas_default_font(void) {
    return (int)((char*)&((struct nk_font_atlas*)0)->default_font - (char*)0);
}
NK_EXPORT int nk_debug_offset_font_handle(void) {
    return (int)((char*)&((struct nk_font*)0)->handle - (char*)0);
}

NK_EXPORT int nk_debug_offset_draw_list(void) {
    return (int)((char*)&((struct nk_context*)0)->draw_list - (char*)0);
}

/* Debug: dump draw command fields from first N commands after nk_convert */
NK_EXPORT void nk_debug_dump_draw_commands(struct nk_context* ctx, struct nk_buffer* cmds, int max_cmds) {
    const struct nk_draw_command* cmd;
    int i = 0;
    nk_draw_foreach(cmd, ctx, cmds) {
        if (i >= max_cmds) break;
        printf("  NativeCmd[%d]: elems=%u, clip=(%.1f,%.1f,%.1f,%.1f), tex=%d\n",
            i, cmd->elem_count,
            cmd->clip_rect.x, cmd->clip_rect.y, cmd->clip_rect.w, cmd->clip_rect.h,
            cmd->texture.id);
        i++;
    }
    fflush(stdout);
}

/* Safe accessor helpers - bypass C# struct layout mismatch */
NK_EXPORT void nk_ctx_set_delta_time(struct nk_context* ctx, float dt) {
    ctx->delta_time_seconds = dt;
}

NK_EXPORT struct nk_input* nk_ctx_get_input(struct nk_context* ctx) {
    return &ctx->input;
}

NK_EXPORT void nk_ctx_set_clipboard(struct nk_context* ctx,
    nk_plugin_copy copy_fn, nk_plugin_paste paste_fn) {
    ctx->clip.copy = copy_fn;
    ctx->clip.paste = paste_fn;
}

NK_EXPORT struct nk_font* nk_font_atlas_add_default_ex(struct nk_font_atlas* atlas, float height) {
    struct nk_font* font = nk_font_atlas_add_default(atlas, height, 0);
    atlas->default_font = font;
    return font;
}
