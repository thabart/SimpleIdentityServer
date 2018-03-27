import React, { Component } from "react";
import { translate } from 'react-i18next';
import { withRouter } from 'react-router-dom';
import Constants from '../../constants';
import $ from 'jquery';
import ReactTable from 'react-table'
import moment from 'moment';

const columns = [
    { Header : 'Event', accessor: 'description' },
    { Header : 'Client', accessor: 'client_id' },
    { Header : 'Created on', accessor: 'created_on' }
];

const errorColumns = [
    { Header : 'Message', accessor: 'message' },
    { Header : 'Created on', accessor: 'created_on' }
];

class ScimTab extends Component {
    constructor(props) {
        super(props);
        this.fetchData = this.fetchData.bind(this);
        this.fetchError = this.fetchError.bind(this);
        this.state = {            
            data : [],
            error : [],
            loading: true,
            errorLoading: true,
            pages: null,
            errorPages : null
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
        url += "/events/.search?filter=where$(Type eq 'scim' and Verbosity eq '0') join$target(groupby$on(AggregateId),aggregate(min with Order)),outer(AggregateId|Order),inner(AggregateId|Order) orderby$on(CreatedOn),order(desc)&startIndex="+startIndex+"&count="+state.pageSize;
        $.get(url).done(function(searchResult) {
            var data = [];
            if (searchResult.content) {
                searchResult.content.forEach(function(log) {
                    var obj = {
                        description: log.Description,
                        created_on: self.getDate(log.CreatedOn),
                        aggregate_id: log.AggregateId,
                        client_id: '-'
                    };
                    data.push(obj);
                });
            }

            var pages = Math.round((searchResult.totalResults + searchResult.itemsPerPage - 1) / searchResult.itemsPerPage);
            self.setState({
                data: data,
                loading: false,
                pages : pages
            });
        }).fail(function() {
            self.setState({
                loading: false
            });
        });
    }

    fetchError(state, instance) {
        var self = this;
        self.setState({
            errorLoading: true
        });

        var url = Constants.eventSourceUrl;
        var startIndex = state.page * state.pageSize;
        url += "/events/.search?filter=where$(Type eq 'scim' and Verbosity eq '1') orderby$on(CreatedOn),order(desc)&startIndex="+startIndex+"&count="+state.pageSize;
        $.get(url).done(function(searchResult) {
            var data = [];
            if (searchResult.content) {
                searchResult.content.forEach(function(log) {
                    var obj = {
                        message: '-',
                        created_on: self.getDate(log.CreatedOn)
                    };
                    if (log.Payload) {
                        var requestPayload = JSON.parse(log.Payload);
                        if (requestPayload && requestPayload.error) {
                            obj['message'] = requestPayload.error;
                        }
                    }
                    data.push(obj);
                });
            }

            var pages = Math.round((searchResult.totalResults + searchResult.itemsPerPage - 1) / searchResult.itemsPerPage);
            self.setState({
                error: data,
                errorLoading: false,
                errorPages : pages
            });
        }).fail(function() {
            self.setState({
                errorLoading: false
            });
        });
    }
    
    render() {
        var self = this;
        const { t } = self.props;
        return (<div className="row">
            <div className="col-md-6">
                <div className="card">
                    <div className="header">
                        <h4>{t('scimLogsTitle')}</h4>
                    </div>
                    <div className="body" style={{ overflow: "hidden" }}>
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
                                        self.props.history.push("/viewlog/" + selectedEvent.aggregate_id);
                                    }
                                }
                            }}
                            className="-striped -highlight"
                        />
                    </div>
                </div>
            </div>
            <div className="col-md-6">
                <div className="card">
                    <div className="header">
                        <h4>{t('scimErrorsTitle')}</h4>
                    </div>
                    <div className="body" style={{ overflow: "hidden" }}>
                        <ReactTable
                            data={this.state.error}
                            loading={this.state.errorLoading}
                            onFetchData={this.fetchError}
                            pages={this.state.errorPages}
                            columns={errorColumns}
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


}

export default translate('common', { wait: process && !process.release })(withRouter(ScimTab));