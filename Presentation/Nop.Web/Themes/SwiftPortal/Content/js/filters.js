Vue.filter('date', function (value) {
    if (!value) return '';
    return moment.utc(value).format("MM/DD/YYYY");
});

Vue.filter('amount', function (value, param) {
    if (!value) return param === 2 ? '0.00' : '0';
    return `${Number(value).toLocaleString(undefined, { maximumFractionDigits: 2, minimumFractionDigits: param || param === 0 ? param : 2 })}`;
});