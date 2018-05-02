import $ from 'jquery';
import Constants from '../constants';
import SessionService from './sessionService';

module.exports = {
	/**
	* Search the scopes.
	*/
	search: function(request, type) {
        return new Promise(function (resolve, reject) {
            var data = JSON.stringify(request);
            var session = SessionService.getSession();
            $.ajax({
                url: Constants.apiUrl + '/scopes/'+type+'/.search',
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
	* Get the scope.
	*/
	get: function(id, type) {
		return new Promise(function(resolve, reject) {
            var session = SessionService.getSession();
			$.ajax({
				url: Constants.apiUrl + '/scopes/' + type + '/' +id,
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
    * Add a scope.
    */
    add: function(request, type) {
        return new Promise(function (resolve, reject) {
            var data = JSON.stringify(request);
            var session = SessionService.getSession();
            $.ajax({
                url: Constants.apiUrl + '/scopes/'+type,
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
    * Remove a scope.
    */
    delete: function(id, type) {
        return new Promise(function (resolve, reject) {
            var session = SessionService.getSession();
            $.ajax({
                url: Constants.apiUrl + '/scopes/'+type + '/' + id,
                method: "DELETE",
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
    * Update a scope.
    */
    update: function(request, type) {
        return new Promise(function (resolve, reject) {
            var data = JSON.stringify(request);
            var session = SessionService.getSession();
            $.ajax({
                url: Constants.apiUrl + '/scopes/'+type,
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
    }
};