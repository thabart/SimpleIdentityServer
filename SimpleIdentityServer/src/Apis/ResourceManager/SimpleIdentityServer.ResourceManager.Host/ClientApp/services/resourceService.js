import $ from 'jquery';
import Constants from '../constants';

module.exports = {
	/**
	* Search the resources.
	*/
	search: function(request, type) {
        return new Promise(function (resolve, reject) {
            var data = JSON.stringify(request);
            $.ajax({
                url: Constants.apiUrl + '/resources/.search',
                method: "POST",
                data: data,
                contentType: 'application/json'
            }).then(function (data) {
                resolve(data);
            }).fail(function (e) {
                reject(e);
            });
		});
	},
    /**
    * Get the authorization policies.
    */
    getAuthPolicies: function(id) {
        return new Promise(function (resolve, reject) {
            $.ajax({
                url: Constants.apiUrl + '/resources/' + id + '/policies',
                method: "GET"
            }).then(function (data) {
                resolve(data);
            }).fail(function (e) {
                reject(e);
            });
        });        
    },
    /**
    * Get the resource.
    */
    get: function(id) {
        return new Promise(function (resolve, reject) {
            $.ajax({
                url: Constants.apiUrl + '/resources/' + id,
                method: "GET"
            }).then(function (data) {
                resolve(data);
            }).fail(function (e) {
                reject(e);
            });
        });        
    },
    /**
    * Remove the user.
    */
    delete: function(id, type) {

    }
};