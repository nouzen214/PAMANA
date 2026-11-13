mergeInto(LibraryManager.library, {
    IsMobileBrowser: function () {
        var ua = (navigator.userAgent || navigator.vendor || window.opera || "").toLowerCase();
        return (/iphone|ipad|ipod|android|blackberry|iemobile|opera mini|mobile/i.test(ua));
    }
});
