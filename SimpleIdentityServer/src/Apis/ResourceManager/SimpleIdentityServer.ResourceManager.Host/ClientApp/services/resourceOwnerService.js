import $ from 'jquery';
import Constants from '../constants';
import SessionService from './sessionService';

module.exports = {
	/**
	* Search the users.
	*/
	search: function(request, type) {
        return new Promise(function (resolve, reject) {
            var data = JSON.stringify(request);
            var session = SessionService.getSession();
            $.ajax({
                url: Constants.apiUrl + '/resourceowners/.search',
                method: "POST",
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
	* Get the user.
	*/
	get: function(id) {
		return new Promise(function(resolve, reject) {
            var session = SessionService.getSession();
			$.ajax({
				url: Constants.apiUrl + '/resourceowners/' + id,
                method: "GET",
                headers: {
                	"Authorization": "Bearer "+ session.token
                }
			}).then(function(data) {
				resolve(data);
			}).fail(function(e) {
				reject(e);
			});
		});
	},
	/**
	* Remove the user.
	*/
	delete: function(id) {
		return new Promise(function(resolve, reject) {
            var session = SessionService.getSession();
			$.ajax({
				url: Constants.apiUrl + '/resourceowners/' + id,
                method: "DELETE",
                headers: {
                	"Authorization": "Bearer "+ session.token
                }
			}).then(function(data) {
				resolve(data);
			}).fail(function(e) {
				reject(e);
			});
		});
	},
    /**
    * Add a client.
    */
    add: function(request) {
        return new Promise(function (resolve, reject) {
            var data = JSON.stringify(request);
            var session = SessionService.getSession();
            $.ajax({
				url: Constants.apiUrl + '/resourceowners/' + id,
                method: "POST",
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
};