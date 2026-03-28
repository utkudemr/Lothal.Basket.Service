/// <reference types="C:/Users/utkudemir/.gemini/antigravity/scratch/Lothal.Basket.Service/src/UI/lothal-admin-ui/node_modules/@vue/language-core/types/template-helpers.d.ts" />
/// <reference types="C:/Users/utkudemir/.gemini/antigravity/scratch/Lothal.Basket.Service/src/UI/lothal-admin-ui/node_modules/@vue/language-core/types/props-fallback.d.ts" />
import { useProductStore } from '../stores/products';
const store = useProductStore();
const emit = defineEmits(['edit-product', 'adjust-stock']);
const deleteProduct = async (barcode) => {
    if (confirm('Are you sure you want to delete this product?')) {
        await store.deleteProduct(barcode);
    }
};
const getStockBadgeClass = (qty) => {
    if (qty <= 0)
        return 'badge-danger';
    if (qty < 10)
        return 'badge-warning';
    return 'badge-success';
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
__VLS_asFunctionalElement1(__VLS_intrinsics.div, __VLS_intrinsics.div)({
    ...{ class: "table-container glass-card" },
});
/** @type {__VLS_StyleScopedClasses['table-container']} */ ;
/** @type {__VLS_StyleScopedClasses['glass-card']} */ ;
__VLS_asFunctionalElement1(__VLS_intrinsics.table, __VLS_intrinsics.table)({});
__VLS_asFunctionalElement1(__VLS_intrinsics.thead, __VLS_intrinsics.thead)({});
__VLS_asFunctionalElement1(__VLS_intrinsics.tr, __VLS_intrinsics.tr)({});
__VLS_asFunctionalElement1(__VLS_intrinsics.th, __VLS_intrinsics.th)({});
__VLS_asFunctionalElement1(__VLS_intrinsics.th, __VLS_intrinsics.th)({});
__VLS_asFunctionalElement1(__VLS_intrinsics.th, __VLS_intrinsics.th)({});
__VLS_asFunctionalElement1(__VLS_intrinsics.th, __VLS_intrinsics.th)({});
__VLS_asFunctionalElement1(__VLS_intrinsics.th, __VLS_intrinsics.th)({});
__VLS_asFunctionalElement1(__VLS_intrinsics.th, __VLS_intrinsics.th)({});
__VLS_asFunctionalElement1(__VLS_intrinsics.tbody, __VLS_intrinsics.tbody)({});
for (const [product] of __VLS_vFor((__VLS_ctx.store.products))) {
    __VLS_asFunctionalElement1(__VLS_intrinsics.tr, __VLS_intrinsics.tr)({
        key: (product.barcode),
        ...{ class: "animate-fade-in" },
    });
    /** @type {__VLS_StyleScopedClasses['animate-fade-in']} */ ;
    __VLS_asFunctionalElement1(__VLS_intrinsics.td, __VLS_intrinsics.td)({});
    __VLS_asFunctionalElement1(__VLS_intrinsics.code, __VLS_intrinsics.code)({});
    (product.barcode);
    __VLS_asFunctionalElement1(__VLS_intrinsics.td, __VLS_intrinsics.td)({});
    __VLS_asFunctionalElement1(__VLS_intrinsics.strong, __VLS_intrinsics.strong)({});
    (product.name);
    __VLS_asFunctionalElement1(__VLS_intrinsics.td, __VLS_intrinsics.td)({});
    (product.class);
    (product.color);
    (product.size);
    __VLS_asFunctionalElement1(__VLS_intrinsics.td, __VLS_intrinsics.td)({});
    (product.price.toFixed(2));
    __VLS_asFunctionalElement1(__VLS_intrinsics.td, __VLS_intrinsics.td)({});
    if (product.stock) {
        __VLS_asFunctionalElement1(__VLS_intrinsics.div, __VLS_intrinsics.div)({
            ...{ class: "stock-info" },
        });
        /** @type {__VLS_StyleScopedClasses['stock-info']} */ ;
        __VLS_asFunctionalElement1(__VLS_intrinsics.span, __VLS_intrinsics.span)({
            ...{ class: "badge" },
            ...{ class: (__VLS_ctx.getStockBadgeClass(product.stock.warehouseQuantity)) },
        });
        /** @type {__VLS_StyleScopedClasses['badge']} */ ;
        (product.stock.warehouseQuantity);
        __VLS_asFunctionalElement1(__VLS_intrinsics.span, __VLS_intrinsics.span)({});
        (product.stock.availableQuantity ?? '--');
    }
    else {
        __VLS_asFunctionalElement1(__VLS_intrinsics.div, __VLS_intrinsics.div)({
            ...{ class: "text-dim" },
        });
        /** @type {__VLS_StyleScopedClasses['text-dim']} */ ;
    }
    __VLS_asFunctionalElement1(__VLS_intrinsics.td, __VLS_intrinsics.td)({});
    __VLS_asFunctionalElement1(__VLS_intrinsics.div, __VLS_intrinsics.div)({
        ...{ class: "actions" },
    });
    /** @type {__VLS_StyleScopedClasses['actions']} */ ;
    __VLS_asFunctionalElement1(__VLS_intrinsics.button, __VLS_intrinsics.button)({
        ...{ onClick: (...[$event]) => {
                __VLS_ctx.emit('adjust-stock', product);
                // @ts-ignore
                [store, getStockBadgeClass, emit,];
            } },
        ...{ class: "btn btn-secondary btn-icon" },
        title: "Adjust Stock",
    });
    /** @type {__VLS_StyleScopedClasses['btn']} */ ;
    /** @type {__VLS_StyleScopedClasses['btn-secondary']} */ ;
    /** @type {__VLS_StyleScopedClasses['btn-icon']} */ ;
    __VLS_asFunctionalElement1(__VLS_intrinsics.button, __VLS_intrinsics.button)({
        ...{ onClick: (...[$event]) => {
                __VLS_ctx.emit('edit-product', product);
                // @ts-ignore
                [emit,];
            } },
        ...{ class: "btn btn-secondary btn-icon" },
        title: "Edit Product",
    });
    /** @type {__VLS_StyleScopedClasses['btn']} */ ;
    /** @type {__VLS_StyleScopedClasses['btn-secondary']} */ ;
    /** @type {__VLS_StyleScopedClasses['btn-icon']} */ ;
    __VLS_asFunctionalElement1(__VLS_intrinsics.button, __VLS_intrinsics.button)({
        ...{ onClick: (...[$event]) => {
                __VLS_ctx.deleteProduct(product.barcode);
                // @ts-ignore
                [deleteProduct,];
            } },
        ...{ class: "btn btn-danger btn-icon" },
        title: "Delete Product",
    });
    /** @type {__VLS_StyleScopedClasses['btn']} */ ;
    /** @type {__VLS_StyleScopedClasses['btn-danger']} */ ;
    /** @type {__VLS_StyleScopedClasses['btn-icon']} */ ;
    // @ts-ignore
    [];
}
if (__VLS_ctx.store.loading) {
    __VLS_asFunctionalElement1(__VLS_intrinsics.div, __VLS_intrinsics.div)({
        ...{ class: "loading-overlay" },
    });
    /** @type {__VLS_StyleScopedClasses['loading-overlay']} */ ;
    __VLS_asFunctionalElement1(__VLS_intrinsics.div, __VLS_intrinsics.div)({
        ...{ class: "spinner" },
    });
    /** @type {__VLS_StyleScopedClasses['spinner']} */ ;
}
// @ts-ignore
[store,];
const __VLS_export = (await import('vue')).defineComponent({
    emits: {},
});
export default {};
//# sourceMappingURL=ProductTable.vue.js.map