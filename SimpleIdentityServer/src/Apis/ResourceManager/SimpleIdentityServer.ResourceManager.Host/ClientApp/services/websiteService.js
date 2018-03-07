import $ from 'jquery';
import Constants from '../constants';

module.exports = {
    authenticate: function (login, password) {
        return new Promise(function (resolve, reject) {
            var data = JSON.stringify({ login: login, password: password });
            $.ajax(Constants.baseUrl + '/Home/Authenticate', {
                type: 'POST',
                contentType: 'application/json',
                data: data
            }).then(function (data) {
                resolve(data);
            }).fail(function (e) {
                reject(e);
            });
        });
    }
};