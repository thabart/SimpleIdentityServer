import React, { Component } from "react";
import { translate } from 'react-i18next';
import Constants from '../../../constants';
import moment from 'moment';
import $ from 'jquery';
import { Bar } from 'react-chartjs';
import { Grid, CircularProgress } from 'material-ui';

class ChartsTab extends Component {
    constructor(props) {
        super(props);
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
            nbTokenNbWeeks: 1,
            isAuthLoading: true,
            isTokenLoading: true
        };
    }

    getDate(d) {
      return moment(d).format('LLL');
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
        self.setState({
            isAuthLoading: true
        });
        var url = Constants.eventSourceUrl;
        var startDateTime = moment().subtract(nbAuthorizationDataWeeks, 'week').format('YYYY-MM-DD HH:mm');
        var startAuthProcesses = function() {
            return $.get(url + "/events/.search?filter=where$(Type eq 'openid' and Key eq 'auth_process_started' and CreatedOn gt '"+startDateTime+"') orderby$on(CreatedOn)");
        };
        var grantedAuthProcesses = function() {
            return $.get(url + "/events/.search?filter=where$(Type eq 'openid' and Key eq 'auth_granted' and CreatedOn gt '"+startDateTime+"') orderby$on(CreatedOn)");
        };
        $.when(startAuthProcesses(), grantedAuthProcesses()).done(function(startAuthProcesses, grantedAuthProcesses) {
            startAuthProcesses = startAuthProcesses[0];
            grantedAuthProcesses = grantedAuthProcesses[0];
            var startProcessDataSet = {
                label: 'Authorization process',
                borderWidth: 1,
                fillColor: '#01d8da',
                data: []
            };
            var authorizationGrantedDataSet = {
                label: 'Granted authorization',
                borderWidth: 1,
                fillColor: '#f4f6f9',
                data: []
            };
            var result = {
                labels: [ ],
                datasets: [ startProcessDataSet, authorizationGrantedDataSet ]
            };

            self.fillDataSet(startProcessDataSet, result.labels, startAuthProcesses);
            self.fillDataSet(authorizationGrantedDataSet, result.labels, grantedAuthProcesses);
            self.setState({
                authorizationData: result,
                isAuthLoading: false
            });
        }).fail(function() {
            self.setState({
                isAuthLoading: false,
                authorizationData: null
            });
        });
    }

    refreshTokenData(nbTokenDataWeeks) {
        var self = this;
        self.setState({
            isTokenLoading: true
        });
        var url = Constants.eventSourceUrl;
        var startDateTime = moment().subtract(nbTokenDataWeeks, 'week').format('YYYY-MM-DD HH:mm');
        var tokensGrantedCall = function() {
            return $.get(url + "/events/.search?filter=where$(Type eq 'openid' and Key eq 'token_granted' and CreatedOn gt '"+startDateTime+"') orderby$on(CreatedOn)");
        };     
        var tokensRevokedCall = function() {
            return $.get(url + "/events/.search?filter=where$(Type eq 'openid' and Key eq 'token_revoked' and CreatedOn gt '"+startDateTime+"') orderby$on(CreatedOn)");
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
            self.setState({
                tokenData: result,
                isTokenLoading: false
            });
        }).fail(function() {
            self.setState({
                tokenData: null,
                isTokenLoading: false
            });
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
        return (<Grid container spacing={8}>
        	<Grid item sm={12} md={6} zeroMinWidth>
        		<div className="card">
                    <div className="header"><h4>{t('reportingAuthTitle')}</h4></div>
			        <div className="body" style={{overflow: "hidden"}}>
                        { this.state.isAuthLoading && (<CircularProgress />) }
                        { (!this.state.isAuthLoading && (!this.state.authorizationData || this.state.authorizationData.labels.length === 0)) && (<span>{t('noData')}</span>) }
                        { (!this.state.isAuthLoading && this.state.authorizationData && this.state.authorizationData.labels.length > 0) && (<Bar data={this.state.authorizationData}/>) }
                    </div>
		      	</div>
        	</Grid>
        	<Grid item sm={12} md={6} zeroMinWidth>
        		<div className="card">
			      <div className="header"><h4>{t('reportingTokenTitle')}</h4></div>
			      <div className="body">
                    { this.state.isTokenLoading && (<CircularProgress />) }
                    { (!this.state.isTokenLoading && (!this.state.tokenData || this.state.tokenData.labels.length === 0)) && (<span>{t('noData')}</span>) }
                    { (!this.state.isTokenLoading && this.state.tokenData && this.state.tokenData.labels.length > 0) && (<Bar data={this.state.tokenData}/>) }
			      </div>
		      	</div>
        	</Grid>
        </Grid>);
    }

    componentDidMount() {
        this.refreshAuthorizationData(1);
        this.refreshTokenData(1);
    }
}

export default translate('common', { wait: process && !process.release })(ChartsTab);