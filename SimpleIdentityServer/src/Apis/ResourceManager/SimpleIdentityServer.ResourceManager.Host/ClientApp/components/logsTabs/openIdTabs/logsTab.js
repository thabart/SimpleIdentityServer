import React, { Component } from "react";
import { translate } from 'react-i18next';
import { withRouter } from 'react-router-dom';
import Constants from '../../../constants';
import $ from 'jquery';
import moment from 'moment';

import Table, { TableBody, TableCell, TableHead, TableRow, TableFooter, TablePagination, TableSortLabel } from 'material-ui/Table';
import { Popover, IconButton, Menu, MenuItem, Checkbox, TextField, Select, Avatar, Grid } from 'material-ui';
import { DatePicker } from 'material-ui-pickers';
import MuiPickersUtilsProvider from 'material-ui-pickers/utils/MuiPickersUtilsProvider'
import MomentUtils from 'material-ui-pickers/utils/moment-utils';
import MoreVert from '@material-ui/icons/MoreVert';
import Delete from '@material-ui/icons/Delete';

class LogsTab extends Component {
    constructor(props) {
        super(props);
        this.refreshLogs = this.refreshLogs.bind(this);
        this.refreshErrors = this.refreshErrors.bind(this);
        this.handleLogChangeFilter = this.handleLogChangeFilter.bind(this);
        this.handleLogCreatedOnChange = this.handleLogCreatedOnChange.bind(this);
        this.getLogsUrl = this.getLogsUrl.bind(this);
        this.handleSortLog = this.handleSortLog.bind(this);
        this.handleChangePage = this.handleChangePage.bind(this);
        this.handleChangeRowsPage = this.handleChangeRowsPage.bind(this);
        this.state = {
            loading: true,
        	data : [],
            page: 0,
            pageSize: 5,
            count: 0,
            logOrderBy: '',
            logOrder: 'asc',
            selectedLogEventDescription: '',
            selectedLogCreatedOn: moment().subtract(1, "years"),
            errorData: [],
            errorLoading: true,
            errorPages: null
        };
    }

    /**
    * Get formatted date.
    */
    getDate(d) {
      return moment(d).format('LLL');
    }

    /**
    * Refresh the logs.
    */
    refreshLogs() {
        var self = this;
        self.setState({
            loading: true
        });
        var url = this.getLogsUrl();
        $.get(url).done(function(searchResult) {
            var data = [];
            if (searchResult.content) {
                searchResult.content.forEach(function(log) {
                    var obj = {
                        description: log.Description,
                        created_on: self.getDate(log.CreatedOn),
                        aggregate_id: log.AggregateId,
                        client_id: '-',
                        isSelected: false
                    };
                    if (log.Payload) {
                        var requestPayload = JSON.parse(log.Payload);
                        if (requestPayload && requestPayload.ClientId) {
                            obj['client_id'] = requestPayload.ClientId;
                        }
                    }
                    data.push(obj);
                });
            }

            self.setState({
                isLoading: false,
                data: data,
                count: searchResult.totalResults
            });
        }).fail(function() {
            self.setState({
                loading: false,
                data: []
            });
        });
    }

    /**
    * Refresh the errors.
    */
    refreshErrors() {

    }

    /**
    * When "eventDescription" is modified then refresh the OPENID logs.
    */
    handleLogChangeFilter(e) {        
        var self = this;
        self.setState({
            [e.target.name]: e.target.value
        }, () => {
            self.refreshLogs();
        });
    }

    /**
    * When "createdOn" is modified then refresh the OPENID logs.
    */
    handleLogCreatedOnChange(date) {
        var self = this;
        self.setState({
            selectedLogCreatedOn: date
        }, () => {
            self.refreshLogs();
        });
    }

    /**
    * When columns are sorted then refresh the OPENID logs.
    */
    handleSortLog(colName) {
        var self = this;
        var logOrder = this.state.logOrder === 'asc' ? 'desc' : 'asc';
        if (this.state.logOrderBy !== colName) {
            logOrder = 'asc';
        }

        this.setState({
            logOrderBy: colName,
            logOrder: logOrder
        }, () => {
            self.refreshLogs();
        });
    }

    /**
    * Build the logs URL.
    */
    getLogsUrl() {
        var getColumnName = function(id) {
            if (id === 'description') {
                return 'Description';
            }

            if (id === 'created_on') {
                return 'CreatedOn';
            }

            return null;
        };

        var url = Constants.eventSourceUrl;
        var startIndex = this.state.page * this.state.pageSize;
        var orderBy = "orderby$on(CreatedOn),order(desc)";
        if (this.state.logOrderBy && this.state.logOrderBy !== '') {
            var columnName = getColumnName(this.state.logOrderBy);
            var orderName = this.state.logOrder;
            orderBy = "orderby$on("+columnName+"),order("+orderName+")";
        }

        var filterValue = "Type eq 'simpleidserver' and Verbosity eq '0'";
        var conds = [];
        if (this.state.selectedLogEventDescription && this.state.selectedLogEventDescription !== '') {
            var columnName = getColumnName('description');
            var value = this.state.selectedLogEventDescription;
            var equalitySign = 'co';
            conds.push(columnName + " " + equalitySign + " '"+value + "'");
        }


        if (this.state.selectedLogCreatedOn && this.state.selectedLogCreatedOn !== '') {
            var columnName = getColumnName('created_on');
            var value = this.state.selectedLogCreatedOn.format("YYYY-MM-DD");
            var equalitySign = 'gt';
            conds.push(columnName + " " + equalitySign + " '"+value + "'");
        }

        if (conds.length > 0) {
            filterValue = "Type eq 'simpleidserver' and Verbosity eq '0' and " + conds.join(' and ');            
        }

        url += "/events/.search?filter=where$("+filterValue+") join$target(groupby$on(AggregateId),aggregate(min with Order)),outer(AggregateId|Order),inner(AggregateId|Order) "+orderBy+"&startIndex="+startIndex+"&count="+this.state.pageSize;
        return url;
    }

    /**
    * When the page is changed then refresh the OPENID logs.
    */
    handleChangePage(evt, page) {
        var self = this;
        self.setState({
            page: page
        }, () => {
            self.refreshLogs();
        });
    }

    /**
    * When the number of records is changed then refresh the OPENID logs.
    */
    handleChangeRowsPage(evt) {
        var self = this;
        self.setState({
            pageSize: evt.target.value
        }, () => {
            self.refreshLogs();
        });
    }

    /*

    fetchData(state, instance) {
    	var self = this;
    	self.setState({
    		loading: true
    	});

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
        }

        var getValue = function(filter) {
            if (filter.id === 'created_on') {
                return filter.value.format("YYYY-MM-DD");
            }

            return filter.value;
        }

        var getEqualitySign = function(filter) {
            if (filter.id === 'created_on') {
                return 'gt';
            }

            return 'co';
        }

    	var url = Constants.eventSourceUrl;
    	var startIndex = state.page * state.pageSize;
        var orderBy = "orderby$on(CreatedOn),order(desc)";
        if (state.sorted && state.sorted.length > 0) {
            var firstSort = state.sorted[0];
            var columnName = getColumnName(firstSort.id);
            var orderName = getOrderName(firstSort.desc);
            orderBy = "orderby$on("+columnName+"),order("+orderName+")";
        }

        var filterValue = "Type eq 'simpleidserver' and Verbosity eq '0'";
        if (state.filtered && state.filtered.length > 0) {
            var conds = [];
            state.filtered.forEach(function(filter) {
                var columnName = getColumnName(filter.id);
                var value = getValue(filter);
                var equalitySign = getEqualitySign(filter);
                var cond = columnName + " " + equalitySign + " '"+value + "'";
                conds.push(cond);
            });

            filterValue = "Type eq 'simpleidserver' and Verbosity eq '0' and " + conds.join(' and ');
        }

    	url += "/events/.search?filter=where$("+filterValue+") join$target(groupby$on(AggregateId),aggregate(min with Order)),outer(AggregateId|Order),inner(AggregateId|Order) "+orderBy+"&startIndex="+startIndex+"&count="+state.pageSize;
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
                    if (log.Payload) {
                        var requestPayload = JSON.parse(log.Payload);
                        if (requestPayload && requestPayload.ClientId) {
                            obj['client_id'] = requestPayload.ClientId;
                        }
                    }
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

    errorFetchData(state, instance) {
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

        var filterValue = "Type eq 'simpleidserver' and Verbosity eq '1'";
        if (state.filtered && state.filtered.length > 0) {
            var firstFilter = state.filtered[0];
            var value = firstFilter.value.format("YYYY-MM-DD");
            filterValue = "Type eq 'simpleidserver' and Verbosity eq '1' and CreatedOn gt '" + value + "'";
        }

        url += "/events/.search?filter=where$("+filterValue+") "+orderBy+"&startIndex=" + startIndex + "&count=" + state.pageSize;
        $.get(url).done(function (searchResult) {
            var data = [];
            if (searchResult.content) {
                searchResult.content.forEach(function (log) {
                    var obj = {
                        code: '-',
                        message: '-',
                        created_on: self.getDate(log.CreatedOn),
                        id: log.Id
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
                errorPages: pages
            });
        }).fail(function () {
            self.setState({
                errorLoading: false
            });
        });
    }
    */

    render() {
        var self = this;
        const { t } = this.props;
        var logRows = [];
        if (self.state.data) {
            self.state.data.forEach(function(record) {
                logRows.push((
                    <TableRow hover role="checkbox" key={record.aggregate_id}>
                        <TableCell>{record.description}</TableCell>
                        <TableCell>{record.client_id}</TableCell>
                        <TableCell>{record.created_on}</TableCell>
                    </TableRow>
                ));
            });
        }
        return (<Grid container spacing={8}>
            <Grid item sm={12} md={6} zeroMinWidth>
                <div className="card">
                    <div className="header">
                        <h4>{t('openIdLogsTitle')}</h4>
                    </div>
                    <div className="body" style={{overflowX: "auto"}}>
                        <Table>
                            <TableHead>
                                <TableRow>
                                    <TableCell sortDirection='asc'>
                                        <TableSortLabel active={self.state.logOrderBy === 'description' ? true : false} direction={self.state.logOrder} onClick={() => self.handleSortLog('description')}>{t('eventDescription')}</TableSortLabel>                                        
                                    </TableCell>
                                    <TableCell>{t('clientId')}</TableCell>
                                    <TableCell sortDirection='asc'>
                                        <TableSortLabel active={self.state.logOrderBy === 'created_on' ? true : false} direction={self.state.logOrder} onClick={() => self.handleSortLog('created_on')}>{t('createdAfter')}</TableSortLabel>                                        
                                    </TableCell>
                                </TableRow>
                            </TableHead>
                            <TableBody>
                                <TableRow>
                                    <TableCell><TextField value={this.state.selectedLogEventDescription} name='selectedLogEventDescription' onChange={this.handleLogChangeFilter} fullWidth={true} placeholder={t('Filter...')}/></TableCell>
                                    <TableCell></TableCell>
                                    <TableCell>
                                        <MuiPickersUtilsProvider utils={MomentUtils}>
                                            <DatePicker value={self.state.selectedLogCreatedOn} onChange={self.handleLogCreatedOnChange} />
                                        </MuiPickersUtilsProvider>
                                    </TableCell>
                                </TableRow>
                                {logRows}
                            </TableBody>
                        </Table>
                        <TablePagination component="div" count={self.state.count} rowsPerPage={self.state.pageSize} page={this.state.page} onChangePage={self.handleChangePage} onChangeRowsPerPage={self.handleChangeRowsPage} />
                    </div>
                </div>
            </Grid>
            <Grid item sm={12} md={6} zeroMinWidth>
                <div className="card">
                    <div className="header">
                        <h4>{t('openIdErrorsTitle')}</h4>
                    </div>
                    <div className="body" style={{overflow: "hidden"}}>
                        <Table>
                            <TableHead>
                                <TableRow>
                                    <TableCell>{t('errorCode')}</TableCell>
                                    <TableCell>{t('errorMessage')}</TableCell>
                                    <TableCell>{t('createdAfter')}</TableCell>
                                </TableRow>
                            </TableHead>
                            <TableBody>
                                <TableRow>
                                    <TableCell></TableCell>
                                    <TableCell></TableCell>
                                    <TableCell></TableCell>
                                </TableRow>
                            </TableBody>
                        </Table>
                         {/*
                         <ReactTable
                            data={this.state.errorData}
                            loading={this.state.errorLoading}
                            onFetchData={this.errorFetchData}
                            pages={this.state.errorPages}
                            columns={errorColumns}
                            defaultPageSize={10}
                            manual={true}
                            filterable={true}
                            showPaginationTop={true}
                            showPaginationBottom={false}
                            sortable={true}
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
                        */}
                    </div>
                </div>
            </Grid>
	    </Grid>);
    }

    componentDidMount() {
        this.refreshLogs();
    }
}

export default translate('common', { wait: process && !process.release })(withRouter(LogsTab));