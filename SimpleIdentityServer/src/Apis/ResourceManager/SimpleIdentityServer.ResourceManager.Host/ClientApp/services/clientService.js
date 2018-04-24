import $ from 'jquery';
import Constants from '../constants';

module.exports = {
	/**
	* Search the clients.
	*/
	search: function(request, type) {
        return new Promise(function (resolve, reject) {
            var data = JSON.stringify(request);
            $.ajax({
                url: Constants.apiUrl + '/clients/'+type+'/.search',
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
	* Get the client.
	*/
	get: function(id) {
		return new Promise(function(resolve, reject) {
            $.get(Constants.apiUrl + '/clients/'+id).then(function(data) {
				resolve(data);
			}).fail(function(e) {
				reject(e);
			})
		});
	}
};