/**
 * jQuery mediaPreview
 *
 * @url         http://www.mewsoft.com/jquery/media-preview/
 * @author      Dr. Ahmed Amin Elsheshtawy, Ph.D. <sales@mewsoft.com>
 * @version     1.0
 * @date        30.05.2011
 */

(function ($) {
    $.fn.mediaPreview = function (options) {
        
		var defaults = {
            offset: 10,
			flvplayer: 'flvplayer.swf',
            loading_text: 'Loading, please wait...'
        };
        
		var options = $.extend(defaults, options);
        
		//insert the preview wrapper html code to the end of the body
        //<div id="mediaPreviewWrapper"><h2 id="mediaPreviewTitle"></h2><div id="mediaPreviewBody"></div></div>
        $('body').append('<div id="mediaPreviewWrapper"></div>');
        $('#mediaPreviewWrapper').append('<h2 id="mediaPreviewTitle"></h2>');
        $('#mediaPreviewWrapper').append('<div id="mediaPreviewBody"></div>');

        return this.each(function () {
            var obj = $(this);
            if (obj.length < 1) return false;
            var previewTimeout;

            function showPreview(wrapper, header, title, src, width, height) {
                var previewHTML = '<img id="mediaPreviewImage" height="' + height + '" width="' + width + '" src="' + src + '" alt=""/>' + loading_html;
                header.innerHTML = title;
                if (title) {
                    $('#mediaPreviewTitle').css('display', 'block');
                } else {
                    $('#mediaPreviewTitle').css('display', 'none');
                }
                document.getElementById('mediaPreviewBody').innerHTML = previewHTML;
                previewProgress = document.getElementById('mediaPreviewProgBar');
                previewImage = document.getElementById('mediaPreviewImage');
                previewImage.onload = function () {
                    previewProgress.style.display = 'none';
                }
                previewTimeout = setTimeout(function () {
                    wrapper.display = 'block'
                }, 250);
            }

            function showFLVPreview(wrapper, header, title, src, width, height) {
			     var flvBlock = '' + 
				 '<object classid="clsid:D27CDB6E-AE6D-11cf-96B8-444553540000" style="display:block" codebase="http://download.macromedia.com/pub/shockwave/cabs/flash/swflash.cab#version=7,0,19,0" id="flvPlayer" name="flvPlayer" width="' + width + '" height="' + height + '">' + 
				 '<param name="allowScriptAccess" value="sameDomain" />' + 
				 '<param name="allowFullScreen" value="true" />' + 
				 '<param name="quality" value="high">' + 
				 '<param name="menu" value="false">' + 
				 '<param id="nameValueFLV" name="movie" value="'+options.flvplayer+'?titleVideo=' + src + '" />' + 
				 '<param name="quality" value="high" />' + 
				 '<param name="bgcolor" value="#010101" />' + 
				 '<embed src="'+options.flvplayer+'?titleVideo=' + src +'" quality="high" menu="false" bgcolor="#010101" width="' + width + '" height="' + height + '" name="video" align="middle" allowScriptAccess="sameDomain" allowFullScreen="true" type="application/x-shockwave-flash" pluginspage="http://www.macromedia.com/go/getflashplayer" /></embed>' +
				 '</object>';

			     var swfBlock = '' + 
				 '<object classid="clsid:D27CDB6E-AE6D-11cf-96B8-444553540000" style="display:block" codebase="http://download.macromedia.com/pub/shockwave/cabs/flash/swflash.cab#version=7,0,19,0" id="swfPlayer" name="swfPlayer" width="' + width + '" height="' + height + '">' + 
				 '<param name="allowScriptAccess" value="sameDomain" />' + 
				 '<param name="allowFullScreen" value="true" />' + 
				 '<param name="quality" value="high">' + 
				 '<param name="menu" value="false">' + 
				 '<param id="nameValueSWF" name="movie" value="'+ src + '" />' + 
				 '<param name="quality" value="high" />' + 
				 '<param name="bgcolor" value="#010101" />' + 
				 '<embed src="' + src +'" quality="high" menu="false" bgcolor="#010101" width="' + width + '" height="' + height + '" name="video" align="middle" allowScriptAccess="sameDomain" allowFullScreen="true" type="application/x-shockwave-flash" pluginspage="http://www.macromedia.com/go/getflashplayer" /></embed>' +
				 '</object>';
				
				var fileExt = src.substr(src.length - 4, 4).toLowerCase();
				switch (fileExt) {
				case '.flv':
						$('#mediaPreviewBody').prepend(flvBlock);
						break;
				case '.swf':
						$('#mediaPreviewBody').prepend(swfBlock);
						break;
				}

                $('#mediaPreviewProgBar').css('display', 'none');
                header.innerHTML = title;

                if (title) {
                    $('#mediaPreviewTitle').css('display', 'block');
                } else {
                    $('#mediaPreviewTitle').css('display', 'none');
                }
                previewTimeout = setTimeout(function () {
                    wrapper.display = 'block'
                }, 250);
            }

            function hidePreview(wrapper, header, image) {
                clearTimeout(previewTimeout);
                $('#mediaPreviewBody').empty();
                wrapper.display = 'none';
            }

            function previewMouseFollow(event, wrapper, width, height, winWidth, winHeight, topOffset) {
                pageX = event.pageX;
                pageY = event.pageY;
                previewOffsetTop = (winHeight - height) / 2;
                previewOffsetLeft = (winWidth - width) / 2;
                correctedTopOffset = previewOffsetTop + topOffset;
                centered = false;
                if (winHeight > height) {
                    if (pageY < correctedTopOffset - options.offset) {
                        pageY = pageY + options.offset;
                    } else if (pageY > correctedTopOffset + height + options.offset) {
                        pageY = pageY - options.offset - height;
                    } else {
                        pageY = correctedTopOffset;
                        centered = true;
                    }
                } else {
                    pageY = topOffset;
                    centered = true;
                }
                if (centered) {
                    if (pageX < winWidth / 2) {
                        pageX += options.offset;
                    } else {
                        pageX = pageX - width - options.offset;
                    }
                } else {
                    if (pageX < previewOffsetLeft - options.offset) {
                        pageX = pageX + options.offset;
                    } else if (pageX > previewOffsetLeft + width + options.offset) {
                        pageX = pageX - width - options.offset;
                    } else {
                        pageX = previewOffsetLeft;
                    }
                }
                wrapper.left = pageX + 'px';
                wrapper.top = pageY + 'px';
            }
            //---------------------------------------------------------------------
            var previewProgress;
            var previewImage;
            var windowObj = new Object();

			var currentWindow = $(window);
            var previewWrapper = document.getElementById('mediaPreviewWrapper').style;
            var previewHeading = document.getElementById('mediaPreviewTitle');
            var loading_html = '<div id="mediaPreviewProgBar">' + options.loading_text + '</div>';

            windowObj.width = currentWindow.width();
            windowObj.height = currentWindow.height();
            windowObj.scrollTop = currentWindow.scrollTop();
			
			currentWindow.resize(function () {
                windowObj.width = $(this).width();
                windowObj.height = $(this).height();
            }).scroll(function () {
                windowObj.scrollTop = $(this).scrollTop();
            });
            //---------------------------------------------------------------------
            obj.each(function () {
                var currentMedia = $(this);
                var widthOffset = 32;
                var heightOffset = 53;
                var display;
                
				var previewObject = new Object;
                previewObject.img = currentMedia.attr('data-src');
                previewObject.width = parseInt(currentMedia.attr('data-width'));
                previewObject.height = parseInt(currentMedia.attr('data-height'));
                previewObject.title = currentMedia.attr('title');

                if (previewObject.width > 0) {
                    currentMedia.hover(

                    function () {
                        var fileExt = previewObject.img.substr(previewObject.img.length - 4, 4).toLowerCase();
                        switch (fileExt) {
                        case '.flv':
						case '.swf':
                            $('#mediaPreviewBody').html(loading_html);
                            previewProgress = document.getElementById('mediaPreviewProgBar');
                            break;
                        default:
                            $('#mediaPreviewBody').html('<img id="mediaPreviewImage" height="" width="" src="" alt=""/>' + loading_html);
                            previewProgress = document.getElementById('mediaPreviewProgBar');
                            previewImage = document.getElementById('mediaPreviewImage');
                            previewImage.onload = function () {
                                previewProgress.style.display = 'none';
                            }
                            break;
                        }
                        previewProgress.style.display = "block";
                        previewWidth = previewObject.width + widthOffset;
                        previewHeight = previewObject.height + heightOffset;
                        if (windowObj.width > previewWidth) {
                            display = true;
                        } else {
                            display = false;
                        }
                        if (display) {
                            switch (fileExt) {
                            case '.flv':
							case '.swf':
                                showFLVPreview(previewWrapper, previewHeading, previewObject.title, previewObject.img, previewObject.width, previewObject.height);
                                break;
                            default:
                                showPreview(
                                previewWrapper, previewHeading, previewObject.title, previewObject.img, previewObject.width, previewObject.height);
                                break;
                            }
                        }
                    }, function () {
                        hidePreview(previewWrapper, previewHeading);
                    }).mousemove(function (event) {
                        if (display) {
                            previewMouseFollow(event, previewWrapper, previewWidth, previewHeight, windowObj.width, windowObj.height, windowObj.scrollTop);
                        }
                    });
                } //if (previewObject.width > 0) {
            }); //obj.each(function() {
            //---------------------------------------------------------------------
        }); //return this.each(function() {
    };
})(jQuery);
