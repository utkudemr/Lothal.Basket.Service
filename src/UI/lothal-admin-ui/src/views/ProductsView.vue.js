/// <reference types="C:/Users/utkudemir/.gemini/antigravity/scratch/Lothal.Basket.Service/src/UI/lothal-admin-ui/node_modules/@vue/language-core/types/template-helpers.d.ts" />
/// <reference types="C:/Users/utkudemir/.gemini/antigravity/scratch/Lothal.Basket.Service/src/UI/lothal-admin-ui/node_modules/@vue/language-core/types/props-fallback.d.ts" />
import { onMounted, ref } from 'vue';
import { useProductStore } from '../stores/products';
import ProductTable from '../components/ProductTable.vue';
import ProductModal from '../components/ProductModal.vue';
import StockAdjustModal from '../components/StockAdjustModal.vue';
const store = useProductStore();
const showProductModal = ref(false);
const showStockModal = ref(false);
const selectedProduct = ref(null);
onMounted(() => {
    store.fetchProducts();
});
const openCreate = () => {
    selectedProduct.value = null;
    showProductModal.value = true;
};
const openEdit = (product) => {
    selectedProduct.value = product;
    showProductModal.value = true;
};
const openStockAdjust = (product) => {
    selectedProduct.value = product;
    showStockModal.value = true;
};
const handleSaveProduct = async (data) => {
    try {
        await store.upsertProduct(data);
        showProductModal.value = false;
    }
    catch (err) {
        // Error handled by store/toast
    }
};
const __VLS_ctx = {
    ...{},
    ...{},
};
let __VLS_components;
let __VLS_intrinsics;
let __VLS_directives;
/** @type {__VLS_StyleScopedClasses['page-header']} */ ;
/** @type {__VLS_StyleScopedClasses['stat']} */ ;
__VLS_asFunctionalElement1(__VLS_intrinsics.main, __VLS_intrinsics.main)({
    ...{ class: "container" },
});
/** @type {__VLS_StyleScopedClasses['container']} */ ;
__VLS_asFunctionalElement1(__VLS_intrinsics.header, __VLS_intrinsics.header)({
    ...{ class: "page-header animate-fade-in" },
});
/** @type {__VLS_StyleScopedClasses['page-header']} */ ;
/** @type {__VLS_StyleScopedClasses['animate-fade-in']} */ ;
__VLS_asFunctionalElement1(__VLS_intrinsics.div, __VLS_intrinsics.div)({});
__VLS_asFunctionalElement1(__VLS_intrinsics.h1, __VLS_intrinsics.h1)({});
__VLS_asFunctionalElement1(__VLS_intrinsics.p, __VLS_intrinsics.p)({
    ...{ class: "text-dim" },
});
/** @type {__VLS_StyleScopedClasses['text-dim']} */ ;
__VLS_asFunctionalElement1(__VLS_intrinsics.button, __VLS_intrinsics.button)({
    ...{ onClick: (__VLS_ctx.openCreate) },
    ...{ class: "btn btn-primary" },
});
/** @type {__VLS_StyleScopedClasses['btn']} */ ;
/** @type {__VLS_StyleScopedClasses['btn-primary']} */ ;
__VLS_asFunctionalElement1(__VLS_intrinsics.span, __VLS_intrinsics.span)({});
__VLS_asFunctionalElement1(__VLS_intrinsics.div, __VLS_intrinsics.div)({
    ...{ class: "stats-overview animate-fade-in" },
    ...{ style: {} },
});
/** @type {__VLS_StyleScopedClasses['stats-overview']} */ ;
/** @type {__VLS_StyleScopedClasses['animate-fade-in']} */ ;
__VLS_asFunctionalElement1(__VLS_intrinsics.div, __VLS_intrinsics.div)({
    ...{ class: "glass-card stat" },
});
/** @type {__VLS_StyleScopedClasses['glass-card']} */ ;
/** @type {__VLS_StyleScopedClasses['stat']} */ ;
__VLS_asFunctionalElement1(__VLS_intrinsics.label, __VLS_intrinsics.label)({});
__VLS_asFunctionalElement1(__VLS_intrinsics.div, __VLS_intrinsics.div)({
    ...{ class: "val" },
});
/** @type {__VLS_StyleScopedClasses['val']} */ ;
(__VLS_ctx.store.products.length);
__VLS_asFunctionalElement1(__VLS_intrinsics.div, __VLS_intrinsics.div)({
    ...{ class: "glass-card stat" },
});
/** @type {__VLS_StyleScopedClasses['glass-card']} */ ;
/** @type {__VLS_StyleScopedClasses['stat']} */ ;
__VLS_asFunctionalElement1(__VLS_intrinsics.label, __VLS_intrinsics.label)({});
__VLS_asFunctionalElement1(__VLS_intrinsics.div, __VLS_intrinsics.div)({
    ...{ class: "val" },
});
/** @type {__VLS_StyleScopedClasses['val']} */ ;
(__VLS_ctx.store.products.filter(p => (p.stock?.warehouseQuantity ?? 0) < 10).length);
const __VLS_0 = ProductTable;
// @ts-ignore
const __VLS_1 = __VLS_asFunctionalComponent1(__VLS_0, new __VLS_0({
    ...{ 'onEditProduct': {} },
    ...{ 'onAdjustStock': {} },
    ...{ style: {} },
}));
const __VLS_2 = __VLS_1({
    ...{ 'onEditProduct': {} },
    ...{ 'onAdjustStock': {} },
    ...{ style: {} },
}, ...__VLS_functionalComponentArgsRest(__VLS_1));
let __VLS_5;
const __VLS_6 = ({ editProduct: {} },
    { onEditProduct: (__VLS_ctx.openEdit) });
const __VLS_7 = ({ adjustStock: {} },
    { onAdjustStock: (__VLS_ctx.openStockAdjust) });
var __VLS_3;
var __VLS_4;
const __VLS_8 = ProductModal;
// @ts-ignore
const __VLS_9 = __VLS_asFunctionalComponent1(__VLS_8, new __VLS_8({
    ...{ 'onClose': {} },
    ...{ 'onSave': {} },
    show: (__VLS_ctx.showProductModal),
    product: (__VLS_ctx.selectedProduct || undefined),
}));
const __VLS_10 = __VLS_9({
    ...{ 'onClose': {} },
    ...{ 'onSave': {} },
    show: (__VLS_ctx.showProductModal),
    product: (__VLS_ctx.selectedProduct || undefined),
}, ...__VLS_functionalComponentArgsRest(__VLS_9));
let __VLS_13;
const __VLS_14 = ({ close: {} },
    { onClose: (...[$event]) => {
            __VLS_ctx.showProductModal = false;
            // @ts-ignore
            [openCreate, store, store, openEdit, openStockAdjust, showProductModal, showProductModal, selectedProduct,];
        } });
const __VLS_15 = ({ save: {} },
    { onSave: (__VLS_ctx.handleSaveProduct) });
var __VLS_11;
var __VLS_12;
const __VLS_16 = StockAdjustModal;
// @ts-ignore
const __VLS_17 = __VLS_asFunctionalComponent1(__VLS_16, new __VLS_16({
    ...{ 'onClose': {} },
    show: (__VLS_ctx.showStockModal),
    product: (__VLS_ctx.selectedProduct),
}));
const __VLS_18 = __VLS_17({
    ...{ 'onClose': {} },
    show: (__VLS_ctx.showStockModal),
    product: (__VLS_ctx.selectedProduct),
}, ...__VLS_functionalComponentArgsRest(__VLS_17));
let __VLS_21;
const __VLS_22 = ({ close: {} },
    { onClose: (...[$event]) => {
            __VLS_ctx.showStockModal = false;
            // @ts-ignore
            [selectedProduct, handleSaveProduct, showStockModal, showStockModal,];
        } });
var __VLS_19;
var __VLS_20;
// @ts-ignore
[];
const __VLS_export = (await import('vue')).defineComponent({});
export default {};
//# sourceMappingURL=ProductsView.vue.js.map