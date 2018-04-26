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
	}
};