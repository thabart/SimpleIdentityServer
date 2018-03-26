import React, { Component } from "react";
import { translate } from 'react-i18next';
import Constants from '../../../constants';
import ReactTable from 'react-table'
import moment from 'moment';
import $ from 'jquery';
import { Bar } from 'react-chartjs';

const columns = [
    { Header : 'Code', accessor: 'code' },
    { Header : 'Message', accessor: 'message' },
    { Header : 'Created on', accessor: 'created_on' }
];



class ChartsTab extends Component {
    constructor(props) {
        super(props);
        this.errorFetchData = this.errorFetchData.bind(this);
        this.refreshAuthorizationData = this.refreshAuthorizationData.bind(this);
        this.refreshTokenData = this.refreshTokenData.bind(this);
        this.changeAuthorizationDataNbWeeks = this.changeAuthorizationDataNbWeeks.bind(this);
        this.changeTokenDataNbWeeks = this.changeTokenDataNbWeeks.bind(this);
        this.state = {
            errorData: [],
            errorLoading: true,
            errorPages : null,
            authorizationData: null,
            tokenData: null,
            nbAuthorizationDataWeeks: 1,
            nbTokenNbWeeks: 1
        };
    }

    getDate(d) {
      return moment(d).format('LLL');
    }

    errorFetchData(state, instance) {
        var self = this;
        self.setState({
            errorLoading: true
        });

        var url = Constants.eventSourceUrl;
        var startIndex = state.page * state.pageSize;
        url += "/events/.search?filter=where$(Type eq 'simpleidserver' and Verbosity eq '1') orderby$on(CreatedOn),order(desc)&startIndex="+startIndex+"&count="+state.pageSize;
        $.get(url).done(function(searchResult) {
            var data = [];
            if (searchResult.content) {
                searchResult.content.forEach(function(log) {
                    var obj = {
                        Code: '-',
                        Message: '-',
                        created_on: self.getDate(log.CreatedOn)
                    };
                    if (log.Payload) {
                        var requestPayload = JSON.parse(log.Payload);
                        if (requestPayload && requestPayload.Code) {
                            obj['code'] = requestPayload.Code;
                        }

                        if (requestPayload && requestPayload.Message) {
                            obj['message'] = requestPayload.Message;
                        }
                    }
                    data.push(obj);
                });
            }

            var pages = Math.round((searchResult.totalResults + searchResult.itemsPerPage - 1) / searchResult.itemsPerPage);
            self.setState({
                errorData: data,
                errorLoading: false,
                errorPages : pages
            });
        }).fail(function() {
            self.setState({
                errorLoading: false
            });
        });
    }

    fillDataSet(ds, labels, data) {
       if (data.content) {
        data.content.forEach(function(p) {
            var m = moment(p.CreatedOn).format('l');
            var index = labels.indexOf(m);
            if (index === -1) {
                labels.push(m);
            }

            var d = ds.data[index];
                if (d) {
                    ds.data[index] += 1;
                } else {
                    ds.data.push(1);
                }
            });
        }
    }

    refreshAuthorizationData(nbAuthorizationDataWeeks) {
        var self = this;
        var url = Constants.eventSourceUrl;
        var startDateTime = moment().subtract(nbAuthorizationDataWeeks, 'week').format('YYYY-MM-DD HH:mm');
        var startAuthProcesses = function() {
            return $.get(url + "/events/.search?filter=where$(Type eq 'simpleidserver' and Key eq 'auth_process_started' and CreatedOn gt '"+startDateTime+"') orderby$on(CreatedOn)");
        };
        var grantedAuthProcesses = function() {
            return $.get(url + "/events/.search?filter=where$(Type eq 'simpleidserver' and Key eq 'auth_granted' and CreatedOn gt '"+startDateTime+"') orderby$on(CreatedOn)");
        };
        $.when(startAuthProcesses(), grantedAuthProcesses()).done(function(startAuthProcesses, grantedAuthProcesses) {
            startAuthProcesses = startAuthProcesses[0];
            grantedAuthProcesses = grantedAuthProcesses[0];
            var startProcessDataSet = {
                label: 'Authorization process',
                borderWidth: 1,
                fillColor: 'green',
                data: []
            };
            var authorizationGrantedDataSet = {
                label: 'Granted authorization',
                borderWidth: 1,
                fillColor: 'yellow',
                data: []
            };
            var result = {
                labels: [ ],
                datasets: [ startProcessDataSet, authorizationGrantedDataSet ]
            };

            self.fillDataSet(startProcessDataSet, result.labels, startAuthProcesses);
            self.fillDataSet(authorizationGrantedDataSet, result.labels, grantedAuthProcesses);
            self.setState({
                authorizationData: result
            });
        }).fail(function() {

        });
    }

    refreshTokenData(nbTokenDataWeeks) {
        var self = this;
        var url = Constants.eventSourceUrl;
        var startDateTime = moment().subtract(nbTokenDataWeeks, 'week').format('YYYY-MM-DD HH:mm');
        var tokensGrantedCall = function() {
            return $.get(url + "/events/.search?filter=where$(Type eq 'simpleidserver' and Key eq 'token_granted' and CreatedOn gt '"+startDateTime+"') orderby$on(CreatedOn)");
        };     
        var tokensRevokedCall = function() {
            return $.get(url + "/events/.search?filter=where$(Type eq 'simpleidserver' and Key eq 'token_revoked' and CreatedOn gt '"+startDateTime+"') orderby$on(CreatedOn)");
        };
        $.when(tokensGrantedCall(), tokensRevokedCall()).done(function(tokensGranted, tokenRevoked) {
            tokensGranted = tokensGranted[0];
            tokenRevoked = tokenRevoked[0];
            var tokensGrantedDataSet = {
                label: 'Granted',
                borderWidth: 1,
                fillColor: 'green',
                data: []
            };
            var tokensRevokedDataSet = {
                label: 'Revoked',
                borderWidth: 1,
                fillColor: 'red',
                data: []
            };
            var result = {
                labels: [ ],
                datasets: [ tokensGrantedDataSet, tokensRevokedDataSet ]
            };

            self.fillDataSet(tokensGrantedDataSet, result.labels, tokensGranted);
            self.fillDataSet(tokensRevokedDataSet, result.labels, tokenRevoked);
            console.log(result);
            self.setState({
                tokenData: result
            });
        }).fail(function() {

        });
    }

    changeAuthorizationDataNbWeeks(evt) {
        var value = evt.target.value;
        this.setState({
            nbAuthorizationDataWeeks: value
        });
        this.refreshAuthorizationData(value);
    }

    changeTokenDataNbWeeks(evt) {        
        var value = evt.target.value;
        this.setState({
            nbTokenNbWeeks: value
        });
        this.refreshAuthorizationData(value);
    }

    render() {
        var self = this;
        const { t } = self.props;
        return (<div className="row">
        	<div className="col-md-6">
        		<div className="card">
			      <div className="card-header" data-target="#authorization" data-toggle="collapse">Authorization</div>
			      <div className="card-block collapse show" id="authorization" style={{overflow: "hidden"}}>
                    <div className="form-group">
                        <label>{t('selectNbWeeksAuthReport')}</label>
                        <select className="form-control" onChange={this.changeAuthorizationDataNbWeeks} value={this.state.nbAuthorizationDataWeeks}>
                            <option value={1}>{t('oneWeek')}</option>
                            <option value={2}>{t('twoWeek')}</option>
                            <option value={3}>{t('threeWeek')}</option>
                            <option value={4}>{t('fourWeek')}</option>
                            <option value={5}>{t('fiveWeek')}</option>
                        </select>
                    </div>
                    {this.state.authorizationData !== null && (<Bar data={this.state.authorizationData}/>)}
                  </div>
		      	</div>
        	</div>
        	<div className="col-md-6">
        		<div className="card">
			      <div className="card-header" data-target="#token" data-toggle="collapse">{t('tokenReportTitle')}</div>
			      <div className="card-block collapse show" id="token">
                        <div className="form-group">
                            <label>{t('selectNbWeeksTokenReport')}</label>
                            <select className="form-control" onChange={this.changeTokenDataNbWeeks} value={this.state.nbTokenNbWeeks}>
                                <option value={1}>{t('oneWeek')}</option>
                                <option value={2}>{t('twoWeek')}</option>
                                <option value={3}>{t('threeWeek')}</option>
                                <option value={4}>{t('fourWeek')}</option>
                                <option value={5}>{t('fiveWeek')}</option>
                            </select>
                        </div>
                        {this.state.tokenData !== null && (<Bar data={this.state.tokenData}/>)}
			      </div>
		      	</div>
        	</div>
        	<div className="col-md-12">
        		<div className="card">
      				<div className="card-header" data-target="#errors" data-toggle="collapse">{t('simpleIdServerErrorsTitle')}</div>
      				<div className="card-block collapse show" id="errors">
                      <ReactTable
                        data={this.state.errorData}
                        loading={this.state.errorLoading}
                        onFetchData={this.errorFetchData}
                        pages={this.state.errorPages}
                        columns={columns}
                        defaultPageSize={10}
                        manual={true}
                        filterable={false}
                        showPaginationTop={true}
                        showPaginationBottom={false}
                        sortable={false}
                        className="-striped -highlight"
                      />
      				</div>
        		</div>
        	</div>
        </div>);
    }

    componentDidMount() {
        this.refreshAuthorizationData(1);
        this.refreshTokenData(1);
    }
}

export default translate('common', { wait: process && !process.release })(ChartsTab);