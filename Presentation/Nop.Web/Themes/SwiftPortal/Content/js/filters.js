Vue.filter('date', function (value) {
    if (!value) return '';
    return moment.utc(value).format("MM/DD/YYYY");
});