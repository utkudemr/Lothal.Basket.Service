/// <reference types="C:/Users/utkudemir/.gemini/antigravity/scratch/Lothal.Basket.Service/src/UI/lothal-admin-ui/node_modules/@vue/language-core/types/template-helpers.d.ts" />
/// <reference types="C:/Users/utkudemir/.gemini/antigravity/scratch/Lothal.Basket.Service/src/UI/lothal-admin-ui/node_modules/@vue/language-core/types/props-fallback.d.ts" />
import { ref } from 'vue';
import { useProductStore } from '../stores/products';
const props = defineProps();
const emit = defineEmits(['close']);
const store = useProductStore();
const amount = ref(1);
const mode = ref('upsert');
const submitting = ref(false);
const handleAdjust = async () => {
    if (!props.product)
        return;
    submitting.value = true;
    try {
        await store.updateStock(props.product.barcode, amount.value, mode.value);
        emit('close');
    }
    finally {
        submitting.value = false;
    }
};
const __VLS_ctx = {
    ...{},
    ...{},
    ...{},
    ...{},
    ...{},
};
let __VLS_components;
let __VLS_intrinsics;
let __VLS_directives;
/** @type {__VLS_StyleScopedClasses['mode-selector']} */ ;
if (__VLS_ctx.show && __VLS_ctx.product) {
    __VLS_asFunctionalElement1(__VLS_intrinsics.div, __VLS_intrinsics.div)({
        ...{ onClick: (...[$event]) => {
                if (!(__VLS_ctx.show && __VLS_ctx.product))
                    return;
                __VLS_ctx.emit('close');
                // @ts-ignore
                [show, product, emit,];
            } },
        ...{ class: "modal-overlay" },
    });
    /** @type {__VLS_StyleScopedClasses['modal-overlay']} */ ;
    __VLS_asFunctionalElement1(__VLS_intrinsics.div, __VLS_intrinsics.div)({
        ...{ class: "modal-content glass-card animate-fade-in" },
    });
    /** @type {__VLS_StyleScopedClasses['modal-content']} */ ;
    /** @type {__VLS_StyleScopedClasses['glass-card']} */ ;
    /** @type {__VLS_StyleScopedClasses['animate-fade-in']} */ ;
    __VLS_asFunctionalElement1(__VLS_intrinsics.h3, __VLS_intrinsics.h3)({});
    (__VLS_ctx.product.name);
    __VLS_asFunctionalElement1(__VLS_intrinsics.p, __VLS_intrinsics.p)({
        ...{ class: "barcode-sub" },
    });
    /** @type {__VLS_StyleScopedClasses['barcode-sub']} */ ;
    (__VLS_ctx.product.barcode);
    __VLS_asFunctionalElement1(__VLS_intrinsics.div, __VLS_intrinsics.div)({
        ...{ class: "mode-selector" },
    });
    /** @type {__VLS_StyleScopedClasses['mode-selector']} */ ;
    __VLS_asFunctionalElement1(__VLS_intrinsics.button, __VLS_intrinsics.button)({
        ...{ onClick: (...[$event]) => {
                if (!(__VLS_ctx.show && __VLS_ctx.product))
                    return;
                __VLS_ctx.mode = 'upsert';
                // @ts-ignore
                [product, product, mode,];
            } },
        ...{ class: "btn" },
        ...{ class: (__VLS_ctx.mode === 'upsert' ? 'btn-primary' : 'btn-secondary') },
    });
    /** @type {__VLS_StyleScopedClasses['btn']} */ ;
    __VLS_asFunctionalElement1(__VLS_intrinsics.button, __VLS_intrinsics.button)({
        ...{ onClick: (...[$event]) => {
                if (!(__VLS_ctx.show && __VLS_ctx.product))
                    return;
                __VLS_ctx.mode = 'reserve';
                // @ts-ignore
                [mode, mode,];
            } },
        ...{ class: "btn" },
        ...{ class: (__VLS_ctx.mode === 'reserve' ? 'btn-primary' : 'btn-secondary') },
    });
    /** @type {__VLS_StyleScopedClasses['btn']} */ ;
    __VLS_asFunctionalElement1(__VLS_intrinsics.button, __VLS_intrinsics.button)({
        ...{ onClick: (...[$event]) => {
                if (!(__VLS_ctx.show && __VLS_ctx.product))
                    return;
                __VLS_ctx.mode = 'release';
                // @ts-ignore
                [mode, mode,];
            } },
        ...{ class: "btn" },
        ...{ class: (__VLS_ctx.mode === 'release' ? 'btn-primary' : 'btn-secondary') },
    });
    /** @type {__VLS_StyleScopedClasses['btn']} */ ;
    __VLS_asFunctionalElement1(__VLS_intrinsics.div, __VLS_intrinsics.div)({
        ...{ class: "form-group mt-2" },
    });
    /** @type {__VLS_StyleScopedClasses['form-group']} */ ;
    /** @type {__VLS_StyleScopedClasses['mt-2']} */ ;
    __VLS_asFunctionalElement1(__VLS_intrinsics.label, __VLS_intrinsics.label)({});
    __VLS_asFunctionalElement1(__VLS_intrinsics.input)({
        type: "number",
        min: "1",
        required: true,
    });
    (__VLS_ctx.amount);
    if (__VLS_ctx.mode === 'upsert') {
        __VLS_asFunctionalElement1(__VLS_intrinsics.p, __VLS_intrinsics.p)({
            ...{ class: "hint" },
        });
        /** @type {__VLS_StyleScopedClasses['hint']} */ ;
        (__VLS_ctx.amount);
    }
    if (__VLS_ctx.mode === 'reserve') {
        __VLS_asFunctionalElement1(__VLS_intrinsics.p, __VLS_intrinsics.p)({
            ...{ class: "hint" },
        });
        /** @type {__VLS_StyleScopedClasses['hint']} */ ;
        (__VLS_ctx.amount);
    }
    if (__VLS_ctx.mode === 'release') {
        __VLS_asFunctionalElement1(__VLS_intrinsics.p, __VLS_intrinsics.p)({
            ...{ class: "hint" },
        });
        /** @type {__VLS_StyleScopedClasses['hint']} */ ;
        (__VLS_ctx.amount);
    }
    __VLS_asFunctionalElement1(__VLS_intrinsics.div, __VLS_intrinsics.div)({
        ...{ class: "form-actions" },
    });
    /** @type {__VLS_StyleScopedClasses['form-actions']} */ ;
    __VLS_asFunctionalElement1(__VLS_intrinsics.button, __VLS_intrinsics.button)({
        ...{ onClick: (...[$event]) => {
                if (!(__VLS_ctx.show && __VLS_ctx.product))
                    return;
                __VLS_ctx.emit('close');
                // @ts-ignore
                [emit, mode, mode, mode, mode, amount, amount, amount, amount,];
            } },
        ...{ class: "btn btn-secondary" },
    });
    /** @type {__VLS_StyleScopedClasses['btn']} */ ;
    /** @type {__VLS_StyleScopedClasses['btn-secondary']} */ ;
    __VLS_asFunctionalElement1(__VLS_intrinsics.button, __VLS_intrinsics.button)({
        ...{ onClick: (__VLS_ctx.handleAdjust) },
        ...{ class: "btn btn-primary" },
        disabled: (__VLS_ctx.submitting),
    });
    /** @type {__VLS_StyleScopedClasses['btn']} */ ;
    /** @type {__VLS_StyleScopedClasses['btn-primary']} */ ;
    (__VLS_ctx.submitting ? 'Processing...' : 'Apply Changes');
}
// @ts-ignore
[handleAdjust, submitting, submitting,];
const __VLS_export = (await import('vue')).defineComponent({
    emits: {},
    __typeProps: {},
});
export default {};
//# sourceMappingURL=StockAdjustModal.vue.js.map