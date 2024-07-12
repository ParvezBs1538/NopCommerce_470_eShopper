
$.fn.serializeObject = function () {
  var o = {};
  var a = this.serializeArray();
  $.each(a, function () {
    if (o[this.name] !== undefined) {
      if (!o[this.name].push) {
        o[this.name] = [o[this.name]];
      }
      o[this.name].push(this.value || '');
    } else {
      o[this.name] = this.value || '';
    }
  });
  return o;
};

var AjaxFilter = {
  url: false,
  filteredSpecIds: [],
  filteredAttrIds: [],
  filteredManufacturerIds: [],
  filteredProductTagIds: [],
  filteredProductRatingIds: [],
  filteredVendorIds: [],
  filteredMiscellaneous: [],
  viewMoreSpecId: 0,
  viewMoreOrShowLessClicked: null,
  currentElement: null,
  pageIndex: 0,
  searchElementName: '',
  isSearch: true,
  onlyInStock: false,
  notInStock: false,
  onlyInStockQuantity: 0,
  discounted: false,
  freeShipping: false,
  taxExempt: false,
  newProduct: false,
  productReview: 0,
  viewMoreKeyPair: {},
  specificationAttributeOptionKeyPair: {},
  selectedAttributes: [],
  setIntializer: function (el, id) {
  },
  spinnerTemplate: "<div class=\"spinner products\"><div class=\"bounce1\"></div><div class=\"bounce2\"></div><div class=\"bounce3\"></div></div>",
  spinnerToggle: function (show) {
    displayAjaxLoading(show)
  },
  init: function (url) {
    this.url = url;
  },
  setInitialFilterParameters: function () {
    if ($("#SpecificationOptionIds").val()) {
      var specIds = $("#SpecificationOptionIds").val().split(',').map(function (item) {
        return parseInt(item, 10);
      });
      AjaxFilter.filteredSpecIds = specIds;
    }

    if ($("#ProductAttributeOptionIds").val()) {
      var attrIds = $("#ProductAttributeOptionIds").val().split(',').map(function (item) {
        return item;
      });
      AjaxFilter.filteredAttrIds = attrIds;
    }

    if ($("#ManufacturerOptionIds").val()) {
      var manIds = $("#ManufacturerOptionIds").val().split(',').map(function (item) {
        return parseInt(item, 10);
      });
      AjaxFilter.filteredManufacturerIds = manIds;
    }

    var urlParams = new URLSearchParams(window.location.search);

    if (urlParams.has('productReview')) {
      AjaxFilter.filteredProductRatingIds = urlParams.get('productReview').split(',');
    }

    if ($("#ProductTagIds").val()) {
      var tagIds = $("#ProductTagIds").val().split(',').map(function (item) {
        return parseInt(item, 10);
      });
      AjaxFilter.filteredProductTagIds = tagIds;
    }

    if ($("#OnlyInStock").val()) {
      AjaxFilter.onlyInStock = $("#OnlyInStock").val();
    }
    if ($("#OnlyInStockQuantity").val()) {
      AjaxFilter.onlyInStockQuantity = $("#OnlyInStockQuantity").val();
    }
    if ($("#DiscountedProduct").val()) {
      AjaxFilter.discounted = true;
    }
    if ($("#FreeShipping").val()) {
      AjaxFilter.freeShipping = true;
    }
    if ($("#TaxExempt").val()) {
      AjaxFilter.taxExempt = true;
    }
    if ($("#NewProduct").val()) {
      AjaxFilter.newProduct = true;
    }
    if ($("#ProductReview").val()) {
      AjaxFilter.productReview = $("#ProductReview").val();
    }
  },
  setFilter: function (el, filterBy, isClicked = false) {

    var pageNumber = $("#PageNumber").val();
    if (pageNumber === undefined) {
      $("#PageNumber").val(0);
    }
    else if (filterBy !== "") {
      $("#PageNumber").val(0);
    }

    if (filterBy.startsWith("s")) {
      var specId = $(el).data("option-id");
      if (specId > 0 && $(el).is(":checked")) {
        AjaxFilter.filteredSpecIds.push(specId);
      }
      else {
        const index = AjaxFilter.filteredSpecIds.indexOf(specId);
        if (index > -1) {
          AjaxFilter.filteredSpecIds.splice(index, 1);
          AjaxFilter.removeSelectedFilterItem(el, "specs", specId);
        }
      }
    }
    else if (filterBy === "productattr") {
      var specId = $(el).data("option-id");
      var name = $(el).data("option-name");
      var attrId = `${specId}-${name}`;
      if ($(el).is(":checked")) {
        AjaxFilter.filteredAttrIds.push(attrId);
      }
      else {
        const index = AjaxFilter.filteredAttrIds.indexOf(attrId);
        if (index > -1) {
          AjaxFilter.filteredAttrIds.splice(index, 1);
          AjaxFilter.removeSelectedFilterItem(el, "productattr", attrId);
        }
      }
    }
    else if (filterBy.startsWith("m")) {
      var manId = $(el).data("manufacturer-id");
      if (manId > 0 && $(el).is(":checked")) {
        AjaxFilter.filteredManufacturerIds.push(manId);
      }
      else {
        const index = AjaxFilter.filteredManufacturerIds.indexOf(manId);
        if (index > -1) {
          AjaxFilter.filteredManufacturerIds.splice(index, 1);
          AjaxFilter.removeSelectedFilterItem(el, "ms", manId);
        }
      }
    }
    else if (filterBy === "producttag") {
      var tagId = $(el).data("tag-id");
      if (tagId > 0 && $(el).is(":checked")) {
        AjaxFilter.filteredProductTagIds.push(tagId);
      }
      else {
        const index = AjaxFilter.filteredProductTagIds.indexOf(tagId);
        if (index > -1) {
          AjaxFilter.filteredProductTagIds.splice(index, 1);
          AjaxFilter.removeSelectedFilterItem(el, "tagids", tagId);
        }
      }
    }
    else if (filterBy.startsWith("v")) {
      var venId = $(el).data("vendor-id");
      if (venId > 0 && $(el).is(":checked")) {
        AjaxFilter.filteredVendorIds.push(venId);
      }
      else {
        const index = AjaxFilter.filteredVendorIds.indexOf(venId);
        if (index > -1) {
          AjaxFilter.filteredVendorIds.splice(index, 1);
          AjaxFilter.removeSelectedFilterItem(el, "ven", venId);
        }
      }
    }
    else if (filterBy == 'notinstockprod') {
      if ($(el).is(":checked")) {
        AjaxFilter.notInStock = true;
      }
      else {
        AjaxFilter.notInStock = false;
      }
    }
    else if (filterBy == 'instockprod') {
      if ($(el).is(":checked")) {
        AjaxFilter.onlyInStock = true;
      }
      else {
        AjaxFilter.onlyInStock = false;
      }
    }
    else if (filterBy == 'instockprodquantity') {
      var quantity = $(el).val();
      AjaxFilter.onlyInStockQuantity = quantity;

    }
    else if (filterBy == 'discountedProduct') {
      if ($(el).is(":checked")) {
        AjaxFilter.discounted = true;
        AjaxFilter.filteredMiscellaneous.push("discountedProduct");
      }
      else {
        AjaxFilter.discounted = false;
      }

    }
    else if (filterBy == 'freeShipping') {

      if ($(el).is(":checked")) {
        AjaxFilter.freeShipping = true;
        AjaxFilter.filteredMiscellaneous.push("freeShipping");
      }
      else {
        AjaxFilter.freeShipping = false;
      }

    }
    else if (filterBy == 'taxExempt') {

      if ($(el).is(":checked")) {
        AjaxFilter.taxExempt = true;
        AjaxFilter.filteredMiscellaneous.push("taxExempt");
      }
      else {
        AjaxFilter.taxExempt = false;
      }

    }
    else if (filterBy == 'newProduct') {

      if ($(el).is(":checked")) {
        AjaxFilter.newProduct = true;
        AjaxFilter.filteredMiscellaneous.push("newProduct");
      }
      else {
        AjaxFilter.newProduct = false;
      }

    }
    //else if (filterBy == 'productRating') {
    //  AjaxFilter.filteredProductRatingIds = $(el).val().split(",");
    //}
    else if (filterBy === "productRating") {
      var tagId = $(el).attr("value");
      if (tagId > 0 && $(el).is(":checked")) {
        AjaxFilter.filteredProductRatingIds = [tagId];
        AjaxFilter.addSelectedFilterItem(el, "productRating", tagId);
      }
      else {
        const index = AjaxFilter.filteredProductRatingIds.indexOf(tagId);
        if (index > -1) {
          AjaxFilter.filteredProductRatingIds.splice(index, 1);
          AjaxFilter.removeSelectedFilterItem(el, "productRating", tagId);
        }
      }
    }
    if (isClicked) {
      let parent = $(el).parent();
      let grandParent = $(parent).parent();

      let grandParentId = $(grandParent).attr("id");

      grandParentId = grandParentId.split('-');
      grandParentId = parseInt(grandParentId[1]);
      if ($(el).is(':checked')) {
        console.log('chekced');
        AjaxFilter.specificationAttributeOptionKeyPair[grandParentId] != undefined ? AjaxFilter.specificationAttributeOptionKeyPair[grandParentId]++ : AjaxFilter.specificationAttributeOptionKeyPair[grandParentId] = 1;
      }
      if ($(el).is(':checked') == false) {
        console.log('not checked!');
        AjaxFilter.specificationAttributeOptionKeyPair[grandParentId] != undefined ? AjaxFilter.specificationAttributeOptionKeyPair[grandParentId]-- : AjaxFilter.specificationAttributeOptionKeyPair[grandParentId] = 0;
      }
    }
    this.getProducts(filterBy);

    if (filterBy !== 'reload') {
      $('html, body').animate({
        scrollTop: $(".products-container").offset().top - 120
      }, 500);
    }

  },
  getProducts: function (filterBy) {
    this.spinnerToggle(true);
    var postData = {};
    var minPrice = $("#price-current-min").val();
    var maxPrice = $("#price-current-max").val();
    if (minPrice && maxPrice) {
      postData.FilteredPrice = `${minPrice}-${maxPrice}`;
    }
    postData.PageSize = $("#PageSize").val();
    postData.PageNumber = $("#PageNumber").val();
    postData.SortOption = $("#SortOption").val();
    postData.ViewMode = $("#ViewMode").val();
    postData.CategoryId = $("#CategoryId").val();
    postData.ManufacturerId = $("#ManufacturerId").val();

    postData.SpecificationOptionIds = AjaxFilter.filteredSpecIds.toString();
    postData.ManufacturerIds = AjaxFilter.filteredManufacturerIds.toString();
    postData.ProductTagIds = AjaxFilter.filteredProductTagIds.toString();
    postData.NotInStock = AjaxFilter.notInStock.toString();
    postData.OnlyInStock = AjaxFilter.onlyInStock.toString();
    postData.OnlyInStockQuantity = AjaxFilter.onlyInStockQuantity.toString();

    postData.FreeShipping = AjaxFilter.freeShipping.toString();
    postData.TaxExempt = AjaxFilter.taxExempt.toString();
    postData.DiscountedProduct = AjaxFilter.discounted.toString();
    postData.NewProduct = AjaxFilter.newProduct.toString();
    postData.ProductRatingIds = AjaxFilter.filteredProductRatingIds.join(',');
    postData.ProductAttributeOptionIds = AjaxFilter.filteredAttrIds.join(',');
    postData.VendorIds = AjaxFilter.filteredVendorIds.toString();
    postData.ViewMoreSpecId = AjaxFilter.viewMoreSpecId;
    postData.ViewMoreOrShowLessClicked = AjaxFilter.viewMoreOrShowLessClicked;
    postData.PageIndex = AjaxFilter.viewMoreKeyPair[AjaxFilter.viewMoreSpecId] != undefined ? AjaxFilter.viewMoreKeyPair[AjaxFilter.viewMoreSpecId] : 0;
    postData.SearchElementName = AjaxFilter.searchElementName;
    postData.SelectedSpecificationAttributes = "";
    postData.SelectedSpecificationAttributeOptions = "";

    for (let key in AjaxFilter.specificationAttributeOptionKeyPair) {
      postData.SelectedSpecificationAttributes += key + ",";
      postData.SelectedSpecificationAttributeOptions += AjaxFilter.specificationAttributeOptionKeyPair[key] + ",";
    }

    $.ajax({
      cache: false,
      url: AjaxFilter.viewMoreOrShowLessClicked == null ? this.url : "AjaxFilter/GetAllSpecificationOptions",
      data: { model: postData, typ: filterBy },
      type: 'post',
      success: AjaxFilter.viewMoreOrShowLessClicked == null ? this.reloadFilters : function (response) {

        let optionData = response.SpecificationModel.SpecificationAttributes;
        let hideProductCount = false;
        let html = "";
        if (optionData[0] != undefined) {
          hideProductCount = optionData[0].HideProductCount;
          optionData = optionData[0].SpecificationAttributeOptions;
          let num = AjaxFilter.viewMoreKeyPair[AjaxFilter.viewMoreSpecId];
          //generate dynamic html
          for (let i = 0; i < optionData.length; i++) {
            let checkedState = optionData[i].CheckedState != 0 ? "Checked" : "";
            let activeState = optionData[i].CheckedState != 0 ? "active" : "";

            html = html + `<li class="item" data-id="` + optionData[i].Id + `">
                            <input class="` + optionData[i].Id + ` d-none"
                                    type="checkbox"
                                    data-option-id="` + optionData[i].Id + `"
                                    data-option-name="` + optionData[i].Name + `"
                                    value="` + optionData[i].Id + `" id="specificationModel_SpecificationAttributes_` + response.ViewMoreSpecId + `_` + (i + 1) + (num > 1 ? 30 * num : 0) + `_Id"
                                    `+ checkedState + `
                                    onclick="AjaxFilter.setFilter(this,'specs',true)" />
                            <label for="specificationModel_SpecificationAttributes_` + response.ViewMoreSpecId + `_` + (i + 1) + (num > 1 ? 30 * num : 0) + `_Id">`;

            if (optionData[i].ColorSquaresRgb) {
              html = html + `<span class="square  ` + activeState + `
                                          ` + optionData[i].Id + `"
                                          style="background-color: ` + optionData[i].ColorSquaresRgb + `"
                                          id="square_specificationModel_SpecificationAttributes_` + response.ViewMoreSpecId + `_` + (i + 1) + (num > 1 ? 30 * num : 0) + `_Id"></span>
                                    color = "regular-checkbox"`;
            }
            else {
              html = html + `<span class="square ` + activeState + `
                                          ` + optionData[i].Id + `"
                                          style="background-color: #fff"
                                          id="square_specificationModel_SpecificationAttributes_` + response.ViewMoreSpecId + `_` + (i + 1) + (num > 1 ? 30 * num : 0) + `_Id"></span>`;
            }
            html = html + `<i>` + optionData[i].Name + ` </i>`;
            if (response.SpecificationModel.ShowProductCountInFilter && !hideProductCount) {
              html = html + ` (` + optionData[i].Count + `)`;
            }
            html = html + `</label>
                        </li>`;
          }
        }
        else {
          //AjaxFilter.viewMoreKeyPair[AjaxFilter.viewMoreSpecId] = 1;
          //AjaxFilter.pageIndex = 0;
        }
        AjaxFilter.viewMoreOrShowLessClicked = null;
        //append to dom element
        $('#specid-' + response.ViewMoreSpecId).append(html);

        //restoreScroll();
      }
    });

  },

  addSelectedFilterItem: function (el, type, id) {
    console.log("Discount = ", el, type, id);
    var filterId = $(el).attr("id");
    var filterName = $(el).data("manufacturer-name");
    if (filterName == undefined) {
      filterName = $(el).data("option-name");
    }
    var filterSectionId = $(el).closest(".filter-section").data("id");
    var $selectedFilterList = $(".selected-filter-list-box ul");
    var elm = $(`<li class="selectedfilters-option-${type}-${id} spec-${filterSectionId}" data-option-id=${id} ><p>${filterName}<span class='ajaxfilter-selectedclose' data-option-selector=${filterSectionId} onclick="javascript:AjaxFilter.removeFromSeletectedFilterList(this, '${filterId}')">&times;</span></p></li>`);
    $selectedFilterList.append(elm);
  },
  removeSelectedFilterItem: function (el, type, id) {
    //var $selectedFilterList = $(`.selected-filter-list-box ul .selectedfilters-option-${type}-${id}`);
    //$selectedFilterList.remove();
  },
  removeFromSeletectedFilterList: function (el, id) {
    var filterSectionType = $(el).data("option-selector");

    if (filterSectionType == "productRating") {
      clearRatingFilters();
    }
    else {
      $(`#${id}`).click();
    }
  },
  reloadPages: function () {
    var pager = $('.pager');

    if (pager.length >= 0) {
      $("a", pager).each(function () {
        $(this).click(function (e) {
          e.preventDefault();

          var hrefAttr = $(this).attr("href");
          hrefAttr = hrefAttr.split("pagenumber=")[1];

          if (hrefAttr === undefined) {
            hrefAttr = 1;
          }
          var pageNumber = hrefAttr - 1;
          $("#PageNumber").val(pageNumber);
          AjaxFilter.getProducts("");
        });
      });
    }
  },
  reloadFilters: function (response) {
    AjaxFilter.spinnerToggle(false);

    if (response.Success) {
      if (response.Url.length > 0) {
        history.pushState(null, null, response.Url);
      }

      $(".nop-ajax-filters").html(response.FilterSection);


      if (response.Products) {

        if (AjaxFilter.viewMoreOrShowLessClicked == null) {
          $('.ajax-products').html(response.Products);
          AjaxFilter.reloadPages();
        }
      }

      if (AjaxFilter.viewMoreOrShowLessClicked == "true" || AjaxFilter.viewMoreOrShowLessClicked == "false") {
        AjaxFilter.viewMoreOrShowLessClicked = null;
      }

      //restoreScroll();
    }
  },
  overlayToggle: function (wantShow) {
    if (wantShow) {
      $(".filter-load-content-bg").show();
    }
    else {
      $(".filter-load-content-bg").hide();
    }
  },
  resetSpecificationFilter: function (el, id, isClicked) {
    var removeIds = [];
    $(`.selected-filter-list-box ul .spec-${id}`).each(function () {
      removeIds.push($(this).data('option-id'));
      $(this).remove();
    });

    AjaxFilter.filteredSpecIds = AjaxFilter.filteredSpecIds.filter(function (x) {
      return removeIds.indexOf(x) < 0;
    });

    AjaxFilter.getProducts('');
  },
  resetManufacturerFilter: function (el, id) {
    var removeIds = [];
    $(`.selected-filter-list-box ul .spec-${id}`).each(function () {
      removeIds.push($(this).data('option-id'));
      $(this).remove();
    });
    AjaxFilter.filteredManufacturerIds = AjaxFilter.filteredManufacturerIds.filter(function (x) {
      return removeIds.indexOf(x) < 0;
    });

    AjaxFilter.getProducts('');
  },
  resetProductTagsFilter: function (el, id) {
    var removeIds = [];
    $(`.selected-filter-list-box ul .spec-${id}`).each(function () {
      removeIds.push($(this).data('option-id'));
      $(this).remove();
    });
    AjaxFilter.filteredProductTagIds = AjaxFilter.filteredProductTagIds.filter(function (x) {
      return removeIds.indexOf(x) < 0;
    });
    AjaxFilter.getProducts('');
  },
  clearAll: function (el) {
    AjaxFilter.filteredManufacturerIds = [];
    AjaxFilter.filteredProductRatingIds = [];
    AjaxFilter.filteredProductTagIds = [];
    AjaxFilter.filteredMiscellaneous = [];
    AjaxFilter.filteredSpecIds = [];
    AjaxFilter.onlyInStock = false;
    AjaxFilter.onlyInStockQuantity = 0;
    AjaxFilter.freeShipping = false;
    AjaxFilter.taxExempt = false;
    AjaxFilter.discounted = false;
    AjaxFilter.newProduct = false;
    AjaxFilter.productReview = 0;

    var priceMin = $("#min-price").val();
    var priceMax = $("#max-price").val();

    $("#price-current-min").val(priceMin);
    $("#price-current-max").val(priceMax);

    $("#slider-range").slider("values", 0, priceMin);
    $("#slider-range").slider("values", 1, priceMax);
    $(".selected-filter-list-box ul").html("");
    AjaxFilter.getProducts();
  },
  addSelectedFilterListOnReload: function () {
    $(".selected-filter-list-box ul").empty();
    if ($('.selected-filter-list-box').length < 1) {
      $(`<div class="selected-filter-list-box">
        <div class='applied-title'>
        </div>
        <ul class='applied-filter-items'></ul>
      </div>`).appendTo(".applied-filters");
    }

    $(".filter-section .ajaxfilter-section ul li input:checkbox:checked, .filter-section .ajaxfilter-section ul li input:radio:checked").each(function () {
      var specId = $(this).data("option-id");
      if (specId > 0) { //specification attribute
        AjaxFilter.addSelectedFilterItem(this, "spec", specId);
      }
    });

    $("#manufacturerNavigation .ajaxfilter-section ul li input:checkbox:checked").each(function () {
      //manufacturer
      var manId = $(this).data("manufacturer-id");
      if (manId > 0) {
        AjaxFilter.addSelectedFilterItem(this, "ms", manId);
      }
    });


    $("#productTagNavigation .ajaxfilter-section ul li input:checkbox:checked").each(function () {
      var tagId = $(this).data("product-tag-id");
      if (tagId > 0) {
        AjaxFilter.addSelectedFilterItem(this, "tagids", tagId);
      }
    });

    $("#VendorsNavigation .ajaxfilter-section ul li input:checkbox:checked").each(function () {

      var venId = $(this).data("vendor-id");
      if (venId > 0) {
        AjaxFilter.addSelectedFilterItem(this, "ven", venId);
      }
    });

    $("#productNavigation .ajaxfilter-section input:checkbox:checked").each(function () {

      AjaxFilter.addSelectedFilterItem(this, "mis", $(this).attr('id'));
    });

    if ($(".applied-filter-items li").length && $('.applied-filter-title').length < 1) {
      $('<h5 class="applied-filter-title">Applied Filters</h5>').appendTo('.applied-title');
    }

  }
};

$.urlParam = function (purl, name) {
  var results = new RegExp('[\?&]' + name + '=([^&#]*)').exec(purl);
  if (results !== null) {
    return decodeURI(results[1]) || 0;
  }
};


function restoreScroll() {
  var scrollpos = localStorage.getItem('scrollpos');
  if (scrollpos) window.scrollTo(0, scrollpos);
}


$(document).ready(function () {
  $('#products-orderby').prop('onchange', '').unbind('onchange').change(function (e) {
    $("#SortOption").val(this.value);
    AjaxFilter.setFilter(this, '');
  });

  $('#products-pagesize').prop('onchange', '').unbind('onchange').change(function (e) {
    $("#PageSize").val(this.value);
    AjaxFilter.getProducts("");
  });

  $(".viewmode-icon").on("click", function (e) {
    e.preventDefault();
    if ($(this).hasClass("grid")) {
      $('#ViewMode').val('grid');
      $('.product-list').removeClass('product-list').addClass('product-grid');
      $('.product-grid>.item-grid').addClass('row');
      $('.product-grid>.item-grid .item-box').addClass('col-md-6');
      $('.product-grid>.item-grid .item-box').addClass('col-xl-4');
      $('.product-grid>.item-grid .item-box').addClass('col-xl-4');
      $('.product-grid>.item-grid .item-box').addClass('m-0');
      $('.product-grid>.item-grid .item-box').addClass('my-3');

    } else {
      $('#ViewMode').val('list');
      $('.product-grid').removeClass('product-grid').addClass('product-list');
      $('.product-list>.item-grid').removeClass('row');
      $('.product-list>.item-grid .item-box').removeClass('col-md-6');
      $('.product-list>.item-grid .item-box').removeClass('col-xl-4');
      $('.product-list>.item-grid .item-box').removeClass('col-xl-4');
      $('.product-list>.item-grid .item-box').removeClass('m-0');
      $('.product-list>.item-grid .item-box').removeClass('my-3');
    }
  });

  AjaxFilter.reloadPages();
});

function clearRatingFilters() {
  var newUrl = new URL(window.location.href);
  newUrl.searchParams.delete('productReview');
  console.log(newUrl);
  window.location.href = newUrl;
}