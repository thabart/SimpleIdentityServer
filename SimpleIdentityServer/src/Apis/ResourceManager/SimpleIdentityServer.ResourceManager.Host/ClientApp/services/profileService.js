import $ from 'jquery';
import Constants from '../constants';
import SessionService from './sessionService';

module.exports = {
    /**
    * Update the profile.
    */
    update: function(request) {
        return new Promise(function (resolve, reject) {
            var data = JSON.stringify(request);
            var session = SessionService.getSession();
            $.ajax({
                url: Constants.apiUrl + '/profile',
                method: "PUT",
                data: data,
                contentType: 'application/json',
                headers: {
                    "Authorization": "Bearer "+ session.token
                }
            }).then(function (data) {
                resolve(data);
            }).fail(function (e) {
                reject(e);
            });
        });        
    },
    /**
    * Get the profile.
    */
    get: function(id) {
        return new Promise(function (resolve, reject) {
            var session = SessionService.getSession();
            $.ajax({
                url: Constants.apiUrl + '/profile',
                method: "GET",
                headers: {
                    "Authorization": "Bearer "+ session.token
                }
            }).then(function (data) {
                resolve(data);
            }).fail(function (e) {
                reject(e);
            });
        });        
    }
};