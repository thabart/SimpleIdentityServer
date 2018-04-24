import $ from 'jquery';
import Constants from '../constants';

module.exports = {
	/**
	* Search the scopes.
	*/
	search: function(request, type) {
        return new Promise(function (resolve, reject) {
            var data = JSON.stringify(request);
            $.ajax({
                url: Constants.apiUrl + '/scopes/'+type+'/.search',
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
	* Get the scope.
	*/
	get: function(id) {
		return new Promise(function(resolve, reject) {
            $.get(Constants.apiUrl + '/scopes/'+id).then(function(data) {
				resolve(data);
			}).fail(function(e) {
				reject(e);
			})
		});
	}
};