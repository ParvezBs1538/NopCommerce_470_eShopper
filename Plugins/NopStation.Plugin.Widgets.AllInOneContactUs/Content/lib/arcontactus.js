function arCuGetCookie(t) {
  return document.cookie.length > 0 &&
    ((c_start = document.cookie.indexOf(t + '=')), -1 != c_start)
    ? ((c_start = c_start + t.length + 1),
      (c_end = document.cookie.indexOf(';', c_start)),
      -1 == c_end && (c_end = document.cookie.length),
      unescape(document.cookie.substring(c_start, c_end)))
    : 0;
}
function arCuCreateCookie(t, e, s) {
  var n;
  if (s) {
    var i = new Date();
    i.setTime(i.getTime() + 24 * s * 60 * 60 * 1e3),
      (n = '; expires=' + i.toGMTString());
  } else n = '';
  document.cookie = t + '=' + e + n + '; path=/';
}
function arCuShowMessage(t) {
  if (arCuPromptClosed) return !1;
  void 0 !== arCuMessages[t]
    ? (jQuery('#arcontactus').contactUs('showPromptTyping'),
      (_arCuTimeOut = setTimeout(function () {
        if (arCuPromptClosed) return !1;
        jQuery('#arcontactus').contactUs('showPrompt', {
          content: arCuMessages[t],
        }),
          t++,
          (_arCuTimeOut = setTimeout(function () {
            if (arCuPromptClosed) return !1;
            arCuShowMessage(t);
          }, arCuMessageTime));
      }, arCuTypingTime)))
    : (arCuCloseLastMessage && jQuery('#arcontactus').contactUs('hidePrompt'),
      arCuLoop && arCuShowMessage(0));
}
function arCuShowMessages() {
  setTimeout(function () {
    clearTimeout(_arCuTimeOut), arCuShowMessage(0);
  }, arCuDelayFirst);
}
!(function (t) {
  function e(s, n) {
    (this._initialized = !1),
      (this.settings = null),
      (this.options = t.extend({}, e.Defaults, n)),
      (this.$element = t(s)),
      this.init(),
      (this.x = 0),
      (this.y = 0),
      this._interval,
      (this._menuOpened = !1),
      (this._callbackOpened = !1),
      (this.countdown = null);
  }
  (e.Defaults = {
    align: 'right',
    countdown: 0,
    drag: !1,
    buttonText: 'Contact us',
    buttonSize: 'large',
    menuSize: 'normal',
    items: [],
    iconsAnimationSpeed: 1200,
    theme: '#00a0db',
    buttonIcon:
      '<svg width="46" height="46" viewBox="0 0 46 46" fill="none" xmlns="http://www.w3.org/2000/svg">< path fill- rule= "evenodd" clip - rule="evenodd" d = "M21 18H29C29.552 18 30 17.552 30 17C30 16.448 29.552 16 29 16H21C20.448 16 20 16.448 20 17C20 17.552 20.448 18 21 18Z" fill = "white" /><path fill-rule="evenodd" clip-rule="evenodd" d="M21 14H37C37.552 14 38 13.552 38 13C38 12.448 37.552 12 37 12H21C20.448 12 20 12.448 20 13C20 13.552 20.448 14 21 14Z" fill="white"/><path fill-rule="evenodd" clip-rule="evenodd" d="M21 22H37C37.552 22 38 21.552 38 21C38 20.448 37.552 20 37 20H21C20.448 20 20 20.448 20 21C20 21.552 20.448 22 21 22Z" fill="white"/><path fill-rule="evenodd" clip-rule="evenodd" d="M33 18H37C37.552 18 38 17.552 38 17C38 16.448 37.552 16 37 16H33C32.448 16 32 16.448 32 17C32 17.552 32.448 18 33 18Z" fill="white"/><path d="M9 34C9.55228 34 10 33.5523 10 33C10 32.4477 9.55228 32 9 32C8.44772 32 8 32.4477 8 33C8 33.5523 8.44772 34 9 34Z" fill="white"/><path d="M13 34C13.5523 34 14 33.5523 14 33C14 32.4477 13.5523 32 13 32C12.4477 32 12 32.4477 12 33C12 33.5523 12.4477 34 13 34Z" fill="white"/><path d="M17 34C17.5523 34 18 33.5523 18 33C18 32.4477 17.5523 32 17 32C16.4477 32 16 32.4477 16 33C16 33.5523 16.4477 34 17 34Z" fill="white"/><path fill-rule="evenodd" clip-rule="evenodd" d="M12.268 20.02C12.092 19.039 12 18.03 12 17C12 7.617 19.617 0 29 0C38.383 0 46 7.617 46 17C46 19.689 45.374 22.234 44.261 24.493L45.885 30.176C46.184 31.223 45.892 32.351 45.121 33.121C44.351 33.892 43.223 34.184 42.176 33.885L36.493 32.261C34.233 33.374 31.689 34 29 34C27.97 34 26.961 33.908 25.98 33.732C25.6 40.567 19.93 46 13 46C10.879 46 8.875 45.491 7.107 44.588L4.114 45.785C3 46.231 1.727 45.97 0.879002 45.121C0.0300016 44.273 -0.231 43 0.215 41.886L1.412 38.893C0.508999 37.124 0 35.121 0 33C0 26.07 5.433 20.4 12.268 20.02ZM12.749 22.003C6.794 22.136 2 27.013 2 33C2 34.94 2.503 36.763 3.386 38.346C3.533 38.609 3.553 38.925 3.441 39.205L2.072 42.629C1.923 43 2.01 43.424 2.293 43.707C2.576 43.99 3 44.077 3.371 43.928L6.795 42.559C7.075 42.447 7.391 42.467 7.654 42.614C9.237 43.497 11.06 44 13 44C18.987 44 23.864 39.206 23.997 33.251C18.635 31.6 14.4 27.365 12.749 22.003ZM36.666 30.23C36.419 30.16 36.155 30.187 35.928 30.305C33.856 31.388 31.499 32 29 32C20.721 32 14 25.279 14 17C14 8.721 20.721 2 29 2C37.279 2 44 8.721 44 17C44 19.499 43.388 21.856 42.305 23.928C42.187 24.155 42.16 24.419 42.23 24.666L43.962 30.725C44.061 31.074 43.964 31.45 43.707 31.707C43.45 31.964 43.074 32.061 42.725 31.962L36.666 30.23Z" fill="white"/></svg >',
    closeIcon:
      '<svg width="12" height="13" viewBox="0 0 14 14" version="1.1" xmlns="http://www.w3.org/2000/svg" xmlns:xlink="http://www.w3.org/1999/xlink"><g id="Canvas" transform="translate(-4087 108)"><g id="Vector"><use xlink:href="#path0_fill" transform="translate(4087 -108)" fill="currentColor"></use></g></g><defs><path id="path0_fill" d="M 14 1.41L 12.59 0L 7 5.59L 1.41 0L 0 1.41L 5.59 7L 0 12.59L 1.41 14L 7 8.41L 12.59 14L 14 12.59L 8.41 7L 14 1.41Z"></path></defs></svg>',
  }),
    (e.prototype.init = function () {
      this.destroy(),
        (this.settings = t.extend({}, this.options)),
        this.$element
          .addClass('arcontactus-widget')
          .addClass('arcontactus-message'),
        'left' === this.settings.align
          ? this.$element.addClass('left')
          : this.$element.addClass('right'),
        this.settings.items.length
          ? (this._initCallbackBlock(),
            this._initMessengersBlock(),
            this._initMessageButton(),
            this._initPrompt(),
            this._initEvents(),
            this.startAnimation(),
            this.$element.addClass('active'))
          : console.info('jquery.contactus:no items'),
        (this._initialized = !0),
        this.$element.trigger('arcontactus.init');
    }),
    (e.prototype.destroy = function () {
      if (!this._initialized) return !1;
      this.$element.html(''),
        (this._initialized = !1),
        this.$element.trigger('arcontactus.destroy');
    }),
    (e.prototype._initCallbackBlock = function () { }),
    (e.prototype._initMessengersBlock = function () {
      var e = t('<div>', { class: 'messangers-block' });
      ('normal' !== this.settings.menuSize &&
        'large' !== this.settings.menuSize) ||
        e.addClass('lg'),
        'small' === this.settings.menuSize && e.addClass('sm'),
        this._appendMessengerIcons(e),
        this.$element.append(e);
    }),
    (e.prototype._appendMessengerIcons = function (e) {
      t.each(this.settings.items, function (s) {
        if ('callback' == this.href)
          var n = t('<div>', {
            class: 'messanger call-back ' + (this.class ? this.class : ''),
          });
        else if (
          ((n = t('<a>', {
            class: 'messanger ' + (this.class ? this.class : ''),
            id: this.id ? this.id : null,
            href: this.href,
            target: this.target ? this.target : '_blank',
          })),
            this.onClick)
        ) {
          var i = this;
          n.on('click', function (t) {
            i.onClick(t);
          });
        }
        var a = t('<span>', {
          style: this.color ? 'background-color:' + this.color : null,
        });
        a.append(this.icon),
          n.append(a),
          n.append('<p>' + this.title + '</p>'),
          e.append(n);
      });
    }),
    (e.prototype._initMessageButton = function () {
      var e = this,
        s = t('<div>', {
          class: 'arcontactus-message-button',
          style: this._backgroundStyle(),
        });
      'large' === this.settings.buttonSize && this.$element.addClass('lg'),
        'medium' === this.settings.buttonSize && this.$element.addClass('md'),
        'small' === this.settings.buttonSize && this.$element.addClass('sm');
      var n = t('<div>', { class: 'static' });
      n.append(this.settings.buttonIcon),
        !1 !== this.settings.buttonText
          ? n.append('<p>' + this.settings.buttonText + '</p>')
          : s.addClass('no-text');
      var i = t('<div>', { class: 'callback-state', style: e._colorStyle() });
      i.append(this.settings.callbackStateIcon);
      var a = t('<div>', { class: 'icons hide' }),
        o = t('<div>', { class: 'icons-line' });
      t.each(this.settings.items, function (s) {
        var n = t('<span>', { style: e._colorStyle() });
        n.append(this.icon), o.append(n);
      }),
        a.append(o);
      var r = t('<div>', { class: 'arcontactus-close' });
      r.append(this.settings.closeIcon);
      var c = t('<div>', { class: 'pulsation', style: e._backgroundStyle() }),
        l = t('<div>', { class: 'pulsation', style: e._backgroundStyle() });
      s.append(n).append(i).append(a).append(r).append(c).append(l),
        this.$element.append(s);
    }),
    (e.prototype._initPrompt = function () {
      var e = t('<div>', { class: 'arcontactus-prompt' }),
        s = t('<div>', {
          class: 'arcontactus-prompt-close',
          style: this._colorStyle(),
        });
      s.append(this.settings.closeIcon);
      var n = t('<div>', { class: 'arcontactus-prompt-inner' });
      e.append(s).append(n), this.$element.append(e);
    }),
    (e.prototype._initEvents = function () {
      var e = this.$element,
        s = this;
      e
        .find('.arcontactus-message-button')
        .on('mousedown', function (t) {
          (s.x = t.pageX), (s.y = t.pageY);
        })
        .on('mouseup', function (t) {
          t.pageX === s.x &&
            t.pageY === s.y &&
            (s.toggleMenu(), t.preventDefault());
        }),
        this.settings.drag &&
        (e.draggable(),
          e.get(0).addEventListener(
            'touchmove',
            function (t) {
              var s = t.targetTouches[0];
              (e.get(0).style.left = s.pageX - 25 + 'px'),
                (e.get(0).style.top = s.pageY - 25 + 'px'),
                t.preventDefault();
            },
            !1
          )),
        t(document).on('click', function (t) {
          s.closeMenu();
        }),
        e.on('click', function (t) {
          t.stopPropagation();
        }),
        e.find('.call-back').on('click', function () {
          s.openCallbackPopup();
        }),
        e.find('.callback-countdown-block-close').on('click', function () {
          null != s.countdown &&
            (clearInterval(s.countdown), (s.countdown = null)),
            s.closeCallbackPopup();
        }),
        e.find('.arcontactus-prompt-close').on('click', function () {
          s.hidePrompt();
        });
    }),
    (e.prototype.show = function () {
      this.$element.addClass('active'),
        this.$element.trigger('arcontactus.show');
    }),
    (e.prototype.hide = function () {
      this.$element.removeClass('active'),
        this.$element.trigger('arcontactus.hide');
    }),
    (e.prototype.openMenu = function () {
      var t = this.$element;
      t.find('.messangers-block').hasClass('show-messageners-block') ||
        (this.stopAnimation(),
          t
            .find('.messangers-block, .arcontactus-close')
            .addClass('show-messageners-block'),
          t.find('.icons, .static').addClass('hide'),
          t.find('.pulsation').addClass('stop'),
          (this._menuOpened = !0),
          this.$element.trigger('arcontactus.openMenu'));
    }),
    (e.prototype.closeMenu = function () {
      var t = this.$element;
      t.find('.messangers-block').hasClass('show-messageners-block') &&
        (t
          .find('.messangers-block, .arcontactus-close')
          .removeClass('show-messageners-block'),
          t.find('.icons, .static').removeClass('hide'),
          t.find('.pulsation').removeClass('stop'),
          this.startAnimation(),
          (this._menuOpened = !1),
          this.$element.trigger('arcontactus.closeMenu'));
    }),
    (e.prototype.toggleMenu = function () {
      var t = this.$element;
      if (
        (this.hidePrompt(),
          t.find('.callback-countdown-block').hasClass('display-flex'))
      )
        return !1;
      t.find('.messangers-block').hasClass('show-messageners-block')
        ? this.closeMenu()
        : this.openMenu(),
        this.$element.trigger('arcontactus.toggleMenu');
    }),
    (e.prototype.openCallbackPopup = function () {
      var t = this.$element;
      t.addClass('opened'),
        this.closeMenu(),
        this.stopAnimation(),
        t.find('.icons, .static').addClass('hide'),
        t.find('.pulsation').addClass('stop'),
        t.find('.callback-countdown-block').addClass('display-flex'),
        (this._callbackOpened = !0),
        this.$element.trigger('arcontactus.openCallbackPopup');
    }),
    (e.prototype.closeCallbackPopup = function () {
      var t = this.$element;
      t.removeClass('opened'),
        t.find('.messangers-block').removeClass('show-messageners-block'),
        t.find('.arcontactus-close').removeClass('show-messageners-block'),
        t.find('.icons, .static').removeClass('hide'),
        this.startAnimation(),
        (this._callbackOpened = !1),
        this.$element.trigger('arcontactus.closeCallbackPopup');
    }),
    (e.prototype.startAnimation = function () {
      var t = this.$element,
        e = t.find('.icons-line'),
        s = t.find('.static'),
        n = t.find('.icons-line>span:first-child').width() + 40;
      if ('large' === this.settings.buttonSize)
        var i = 2,
          a = 0;
      'medium' === this.settings.buttonSize && ((i = 4), (a = -2)),
        'small' === this.settings.buttonSize && ((i = 4), (a = -2));
      var o = t.find('.icons-line>span').length,
        r = 0;
      if ((this.stopAnimation(), 0 === this.settings.iconsAnimationSpeed))
        return !1;
      this._interval = setInterval(function () {
        0 === r && (e.parent().removeClass('hide'), s.addClass('hide'));
        var t = 'translate(' + -(n * r + i) + 'px, ' + a + 'px)';
        e.css({ '-webkit-transform': t, '-ms-transform': t, transform: t }),
          ++r > o &&
          (r > o + 1 && (r = 0),
            e.parent().addClass('hide'),
            s.removeClass('hide'),
            (t = 'translate(' + -i + 'px, ' + a + 'px)'),
            e.css({
              '-webkit-transform': t,
              '-ms-transform': t,
              transform: t,
            }));
      }, this.settings.iconsAnimationSpeed);
    }),
    (e.prototype.stopAnimation = function () {
      clearInterval(this._interval);
      var t = this.$element,
        e = t.find('.icons-line'),
        s = t.find('.static');
      e.parent().addClass('hide'), s.removeClass('hide');
      var n = 'translate(-2px, 0px)';
      e.css({ '-webkit-transform': n, '-ms-transform': n, transform: n });
    }),
    (e.prototype.showPrompt = function (t) {
      var e = this.$element.find('.arcontactus-prompt');
      t && t.content && e.find('.arcontactus-prompt-inner').html(t.content),
        e.addClass('active'),
        this.$element.trigger('arcontactus.showPrompt');
    }),
    (e.prototype.hidePrompt = function () {
      this.$element.find('.arcontactus-prompt').removeClass('active'),
        this.$element.trigger('arcontactus.hidePrompt');
    }),
    (e.prototype.showPromptTyping = function () {
      this.$element
        .find('.arcontactus-prompt')
        .find('.arcontactus-prompt-inner')
        .html(''),
        this._insertPromptTyping(),
        this.showPrompt({}),
        this.$element.trigger('arcontactus.showPromptTyping');
    }),
    (e.prototype._insertPromptTyping = function () {
      var e = this.$element.find('.arcontactus-prompt-inner'),
        s = t('<div>', { class: 'arcontactus-prompt-typing' }),
        n = t('<div>');
      s.append(n), s.append(n.clone()), s.append(n.clone()), e.append(s);
    }),
    (e.prototype.hidePromptTyping = function () {
      this.$element.find('.arcontactus-prompt').removeClass('active'),
        this.$element.trigger('arcontactus.hidePromptTyping');
    }),
    (e.prototype._backgroundStyle = function () {
      return 'background-color: ' + this.settings.theme;
    }),
    (e.prototype._colorStyle = function () {
      return 'color: ' + this.settings.theme;
    }),
    (t.fn.contactUs = function (s) {
      var n = Array.prototype.slice.call(arguments, 1);
      return this.each(function () {
        var i = t(this),
          a = i.data('ar.contactus');
        a ||
          ((a = new e(this, 'object' == typeof s && s)),
            i.data('ar.contactus', a)),
          'string' == typeof s && '_' !== s.charAt(0) && a[s].apply(a, n);
      });
    }),
    (t.fn.contactUs.Constructor = e);
})(jQuery);
