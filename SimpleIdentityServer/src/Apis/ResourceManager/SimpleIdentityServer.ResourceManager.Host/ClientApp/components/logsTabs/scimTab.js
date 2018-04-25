import React, { Component } from "react";
import { translate } from 'react-i18next';
import { withRouter } from 'react-router-dom';
import Constants from '../../constants';
import $ from 'jquery';
import ReactTable from 'react-table'
import moment from 'moment';

/*
const columns = [
    { Header : 'Event', accessor: 'description' },
    { Header : 'Created on', accessor: 'created_on', Filter : ({filter, onChange}) => {
        return (<div><span>afterDate</span><DatePicker selected={filter ? filter.value : moment()} onChange={onChange} withPortal /></div>)
    } }
];

const errorColumns = [
    { Header : 'Message', accessor: 'message', filterable: false, sortable: false },
    { Header : 'Created on', accessor: 'created_on', Filter : ({filter, onChange}) => {
        return (<div><span>afterDate</span><DatePicker selected={filter ? filter.value : moment()} onChange={onChange} withPortal /></div>)
    } }
];
*/

class ScimTab extends Component {
    constructor(props) {
        super(props);
        // this.fetchData = this.fetchData.bind(this);
        // this.fetchError = this.fetchError.bind(this);
        this.state = {            
            data : [],
            error : [],
            loading: true,
            errorLoading: true,
            pages: null,
            errorPages : null
        };
    }

    /*
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
        var getColumnName = function(id) {
            if (id === 'description') {
                return 'Description';
            }

            if (id === 'created_on') {
                return 'CreatedOn';
            }

            return null;
        };
        var getOrderName = function(desc) {
            if(!desc) {
                return "asc";
            }

            return "desc";
        };
        var getValue = function(filter) {
            if (filter.id === 'created_on') {
                return filter.value.format("YYYY-MM-DD");
            }

            return filter.value;
        };
        var getEqualitySign = function(filter) {
            if (filter.id === 'created_on') {
                return 'gt';
            }

            return 'co';
        };
        var orderBy = "orderby$on(CreatedOn),order(desc)";
        if (state.sorted && state.sorted.length > 0) {
            var firstSort = state.sorted[0];
            var columnName = getColumnName(firstSort.id);
            var orderName = getOrderName(firstSort.desc);
            orderBy = "orderby$on("+columnName+"),order("+orderName+")";
        }

        var filterValue = "Type eq 'scim' and Verbosity eq '0'";
        if (state.filtered && state.filtered.length > 0) {
            var conds = [];
            state.filtered.forEach(function(filter) {
                var columnName = getColumnName(filter.id);
                var value = getValue(filter);
                var equalitySign = getEqualitySign(filter);
                var cond = columnName + " " + equalitySign + " '"+value + "'";
                conds.push(cond);
            });

            filterValue = "Type eq 'scim' and Verbosity eq '0' and " + conds.join(' and ');
        }

        url += "/events/.search?filter=where$("+filterValue+") join$target(groupby$on(AggregateId),aggregate(min with Order)),outer(AggregateId|Order),inner(AggregateId|Order) "+orderBy+"&startIndex="+startIndex+"&count="+state.pageSize;
        $.get(url).done(function(searchResult) {
            var data = [];
            if (searchResult.content) {
                searchResult.content.forEach(function(log) {
                    var obj = {
                        description: log.Description,
                        created_on: self.getDate(log.CreatedOn),
                        aggregate_id: log.AggregateId
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
        var orderBy = "orderby$on(CreatedOn),order(desc)";
        if (state.sorted && state.sorted.length > 0) {
            var firstSort = state.sorted[0];
            if (!firstSort.desc) {
                orderBy = "orderby$on(CreatedOn),order(asc)";
            }
        }

        var filterValue = "Type eq 'scim' and Verbosity eq '1'";
        if (state.filtered && state.filtered.length > 0) {
            var firstFilter = state.filtered[0];
            var value = firstFilter.value.format("YYYY-MM-DD");
            filterValue = "Type eq 'scim' and Verbosity eq '1' and CreatedOn gt '" + value + "'";
        }

        url += "/events/.search?filter=where$("+filterValue+") "+orderBy+"&startIndex="+startIndex+"&count="+state.pageSize;
        $.get(url).done(function(searchResult) {
            var data = [];
            if (searchResult.content) {
                searchResult.content.forEach(function(log) {
                    var obj = {
                        message: '-',
                        created_on: self.getDate(log.CreatedOn),
                        id: log.Id
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
    */
    render() {
        return (<span></span>);
        /*
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
                            filterable={true}
                            showPaginationTop={true}
                            showPaginationBottom={false}
                            sortable={true}
                            getTrProps={(state, rowInfo, column, instance) => {
                                return {
                                    onClick: function (e, handleOriginal) {
                                        var selectedEvent = rowInfo.original;
                                        self.props.history.push("/viewaggregate/" + selectedEvent.aggregate_id);
                                    }
                                }
                            }}
                            className="-striped -highlight"
                            defaultSorted={[{
                                id: 'created_on',
                                desc: true
                            }]}
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
                            filterable={true}
                            showPaginationTop={true}
                            showPaginationBottom={false}
                            sortable={false}
                            className="-striped -highlight"
                            defaultSorted={[{
                                id: 'created_on',
                                desc: true
                            }]}
                            getTrProps={(state, rowInfo, column, instance) => {
                                return {
                                    onClick: function (e, handleOriginal) {
                                        var selectedEvent = rowInfo.original;
                                        self.props.history.push("/viewlog/" + selectedEvent.id);
                                    }
                                }
                            }}
                        />                        
                    </div>
                </div>
            </div>
        </div>);*/
    }


}

export default translate('common', { wait: process && !process.release })(withRouter(ScimTab));