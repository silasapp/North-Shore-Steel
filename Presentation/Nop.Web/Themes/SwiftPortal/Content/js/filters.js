Vue.filter('date', function (value) {
    return moment.utc(value).format("MM/DD/YYYY");
});