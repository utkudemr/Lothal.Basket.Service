/// <reference types="C:/Users/utkudemir/.gemini/antigravity/scratch/Lothal.Basket.Service/src/UI/lothal-admin-ui/node_modules/@vue/language-core/types/template-helpers.d.ts" />
/// <reference types="C:/Users/utkudemir/.gemini/antigravity/scratch/Lothal.Basket.Service/src/UI/lothal-admin-ui/node_modules/@vue/language-core/types/props-fallback.d.ts" />
import { useProductStore } from '../stores/products';
const store = useProductStore();
const __VLS_ctx = {
    ...{},
    ...{},
};
let __VLS_components;
let __VLS_intrinsics;
let __VLS_directives;
let __VLS_0;
/** @ts-ignore @type {typeof __VLS_components.Transition | typeof __VLS_components.Transition} */
Transition;
// @ts-ignore
const __VLS_1 = __VLS_asFunctionalComponent1(__VLS_0, new __VLS_0({
    name: "toast",
}));
const __VLS_2 = __VLS_1({
    name: "toast",
}, ...__VLS_functionalComponentArgsRest(__VLS_1));
const { default: __VLS_5 } = __VLS_3.slots;
if (__VLS_ctx.store.error) {
    __VLS_asFunctionalElement1(__VLS_intrinsics.div, __VLS_intrinsics.div)({
        ...{ onClick: (...[$event]) => {
                if (!(__VLS_ctx.store.error))
                    return;
                __VLS_ctx.store.error = null;
                // @ts-ignore
                [store, store,];
            } },
        ...{ class: "toast-error" },
    });
    /** @type {__VLS_StyleScopedClasses['toast-error']} */ ;
    __VLS_asFunctionalElement1(__VLS_intrinsics.span, __VLS_intrinsics.span)({
        ...{ class: "icon" },
    });
    /** @type {__VLS_StyleScopedClasses['icon']} */ ;
    __VLS_asFunctionalElement1(__VLS_intrinsics.span, __VLS_intrinsics.span)({
        ...{ class: "msg" },
    });
    /** @type {__VLS_StyleScopedClasses['msg']} */ ;
    (__VLS_ctx.store.error);
    __VLS_asFunctionalElement1(__VLS_intrinsics.span, __VLS_intrinsics.span)({
        ...{ class: "close" },
    });
    /** @type {__VLS_StyleScopedClasses['close']} */ ;
}
// @ts-ignore
[store,];
var __VLS_3;
// @ts-ignore
[];
const __VLS_export = (await import('vue')).defineComponent({});
export default {};
//# sourceMappingURL=Toast.vue.js.map