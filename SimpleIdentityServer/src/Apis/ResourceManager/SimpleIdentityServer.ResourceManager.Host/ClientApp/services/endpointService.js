import $ from 'jquery';
import Constants from '../constants';

module.exports = {
	/**
	* Get all the endpoints.
	*/
	getAll: function() {
		return new Promise(function(resolve, reject) {
			$.get(Constants.apiUrl + '/endpoints').then(function(data) {
				resolve(data);
			}).fail(function(e) {
				reject(e);
			});
		});
	},
	/**
	* Get the endpoint.
	*/
	get: function(id) {
		return new Promise(function(resolve, reject) {
			$.get(Constants.apiUrl + '/endpoints/'+id).then(function(data) {
				resolve(data);
			}).fail(function(e) {
				reject(e);
			})
		});
	}
};