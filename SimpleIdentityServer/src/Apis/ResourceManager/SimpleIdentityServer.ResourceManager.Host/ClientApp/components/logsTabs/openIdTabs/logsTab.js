import React, { Component } from "react";
import { translate } from 'react-i18next';
import Constants from '../../../constants';
import Modal from '../../modal';
import Workflow from '../../workflow';
import $ from 'jquery';
import ReactTable from 'react-table'
import CodeMirror from 'react-codemirror';
import moment from 'moment';
import 'codemirror/mode/javascript/javascript';
import 'codemirror/mode/xml/xml';
import 'codemirror/mode/markdown/markdown';

const columns = [
	{ Header : 'Event', accessor: 'description' },
	{ Header : 'Client', accessor: 'client_id' },
	{ Header : 'Created on', accessor: 'created_on' }
];

class LogsTab extends Component {
    constructor(props) {
        super(props);
        this.fetchData = this.fetchData.bind(this);
        this.displayEvent = this.displayEvent.bind(this);
        this.changePayload = this.changePayload.bind(this);
        this.onCloseModal = this.onCloseModal.bind(this);
        this.state = {
        	data : [],
        	loading: true,
        	pages: null,
        	payload: null
        };
    }

    getDate(d) {
      return moment(d).format('LLL');
    }

    fetchData(state, instance) {
    	var self = this;
    	self.setState({
    		loading: true
    	});

    	var url = Constants.eventSourceUrl;
    	var startIndex = state.page * state.pageSize;
    	url += "/events/.search?filter=where$(Type eq 'simpleidserver' and Verbosity eq '0') join$target(groupby$on(AggregateId),aggregate(min with Order)),outer(AggregateId|Order),inner(AggregateId|Order) orderby$on(CreatedOn),order(desc)&startIndex="+startIndex+"&count="+state.pageSize;
    	$.get(url).done(function(searchResult) {
    		var data = [];
    		if (searchResult.content) {
    			searchResult.content.forEach(function(log) {
    				var obj = {
    					description: log.Description,
    					created_on: self.getDate(log.CreatedOn),
    					aggregate_id: log.AggregateId
    				};
    				if (log.Payload) {
    					var requestPayload = JSON.parse(log.Payload);
    					if (requestPayload && requestPayload.ClientId) {
    						obj['client_id'] = requestPayload.ClientId;
    					}
    				} else {
    					obj['client_id'] = '-';
    				}

    				data.push(obj);
    			});
    		}

    		var pages = (searchResult.totalResults + searchResult.itemsPerPage - 1) / searchResult.itemsPerPage;
    		self.setState({
    			data: data,
    			loading: false,
    			pages : pages
    		});
    	}).fail(function() {

    	});
    }

    displayEvent(evt) {
    	var self = this;
    	self.refs.modal.show();
    	var href = Constants.eventSourceUrl + "/events/.search?filter=where$(AggregateId eq '"+evt.aggregate_id+"') orderby$on(Order),order(asc)";
    	$.get(href).done(function(result) {
    		var data = [];
    		if (result.content && result.content.length > 0) {    			
				var record = { title: result.content[0]['Description'], sub: [], info: 'Created on <b>'+ self.getDate(result.content[0]['CreatedOn']) + '</b>', payload: result.content[0]['Payload'] };
				data.push(record);
				var parentRecord = record;
				for (var i = 1; i < result.content.length; i++) {					
					var e = result.content[i];
					var subRecord = { title: e['Description'], sub: [], info: 'Created on <b>' + self.getDate(e['CreatedOn'])  + '</b>', payload: e['Payload'] };
					parentRecord.sub.push(subRecord);
					parentRecord = subRecord;
				}
    		}

    		self.refs.workflow.show({
    			cell: {
    				width: 150,
    				height: 100,
	    			click: function(e) {
	    				var payload = e.payload;	    	
	    				if (self.state.payload === null) {
		    				self.setState({
		    					payload: e.payload
		    				});
	    				} else {	    					
							self.refs.code.codeMirror.setValue(payload);
	    				}
	    			}
    			},
    			frame: {
    				height: 300
    			},
    			data: data
    		});
    	}).fail(function() {

    	});
    }

    changePayload(e) {
    }

    onCloseModal() {
    	this.setState({
    		payload: null
    	});
    }

    render() {
    	var self = this;
	    return (<div>
	    		  <Modal title="Event" ref="modal" onClose={self.onCloseModal}>
	    		  	<Workflow ref="workflow" />
	    		  	{self.state.payload && self.state.payload !== null && (
		    		  	<CodeMirror ref="code" onChange={self.changePayload} value={this.state.payload} options={{
		    		  		lineNumbers: true,
		    		  		mode: 'application/json',
							matchBrackets: true,							
							autoCloseBrackets: true,
							lineWrapping: true,
		    		  	}} />
	    		  	)}
	    		  </Modal>
				  <ReactTable
				  	data={this.state.data}
				    loading={this.state.loading}
				    onFetchData={this.fetchData}
				    pages={this.state.pages}
				    columns={columns}
	         		defaultPageSize={10}
	         		manual={true}
	         		filterable={false}
	         		showPaginationTop={true}
	         		showPaginationBottom={false}
	         		sortable={false}
	         		getTrProps={(state, rowInfo, column, instance) => {
	         			return {
	         				onClick: function (e, handleOriginal) {
	         					var selectedEvent = rowInfo.original;
	         					self.displayEvent(selectedEvent);
	         				}
	         			}
	         		}}
	          		className="-striped -highlight"
				  />
	    </div>);
    }
}

export default translate('common', { wait: process && !process.release })(LogsTab);