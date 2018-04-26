import React, { Component } from "react";
import { translate } from 'react-i18next';
import { withRouter } from 'react-router-dom';
import Constants from '../../constants';
import $ from 'jquery';
import moment from 'moment';

import Table, { TableBody, TableCell, TableHead, TableRow, TableFooter, TablePagination, TableSortLabel } from 'material-ui/Table';
import { Popover, IconButton, Menu, MenuItem, Checkbox, TextField, Select, Avatar, Grid, CircularProgress } from 'material-ui';
import { DatePicker } from 'material-ui-pickers';
import MuiPickersUtilsProvider from 'material-ui-pickers/utils/MuiPickersUtilsProvider'
import MomentUtils from 'material-ui-pickers/utils/moment-utils';
import Search from '@material-ui/icons/Search';

class LogTables extends Component {
    constructor(props) {
        super(props);
        this.refreshLogs = this.refreshLogs.bind(this);
        this.refreshErrors = this.refreshErrors.bind(this);
        this.handleLogChangeFilter = this.handleLogChangeFilter.bind(this);
        this.handleLogCreatedOnChange = this.handleLogCreatedOnChange.bind(this);
        this.getLogsUrl = this.getLogsUrl.bind(this);
        this.handleChangeValue = this.handleChangeValue.bind(this);
        this.getErrorsUrl = this.getErrorsUrl.bind(this);
        this.handleSortLog = this.handleSortLog.bind(this);
        this.handleChangePage = this.handleChangePage.bind(this);
        this.handleChangeRowsPage = this.handleChangeRowsPage.bind(this);
        this.handleErrorChangePage = this.handleErrorChangePage.bind(this);
        this.handleErrorChangeRowsPage = this.handleErrorChangeRowsPage.bind(this);
        this.handleErrorCreatedOnChange = this.handleErrorCreatedOnChange.bind(this);
        this.handleSortError = this.handleSortError.bind(this);
        this.state = {    
            loading: true,
            errorLoading: false,
            data : [],
            errorData: [],
            page: 0,
            errorPage: 0,
            pageSize: 5,
            errorPageSize: 5,
            count: 0,
            errorCount: 0,
            logOrderBy: '',
            errorOrderBy: '',
            logOrder: 'asc',
            errorOrder: 'asc',
            selectedLogCreatedOn: moment().subtract(1, "years"),
            selectedErrorCreatedOn: moment().subtract(1, "years")
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
                loading: false,
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
        var self = this;
        self.setState({
            errorLoading: true
        });
        var url = this.getErrorsUrl();
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

            self.setState({
                isLoading: false,
                errorData: data,
                errorCount: searchResult.totalResults,
                errorLoading: false
            });
        }).fail(function () {
            self.setState({
                errorLoading: false,
                errorData: [],
                errorLoading: false
            });
        });
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
    * When "createdOn" is modified then refresh the ERROR logs.
    */
    handleErrorCreatedOnChange(date) {
        var self = this;
        self.setState({
            selectedErrorCreatedOn: date
        }, () => {
            self.refreshErrors();
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
    * When columns are sorted then refresh the ERROR logs.
    */
    handleSortError(colName) {
        var self = this;
        var logOrder = this.state.logOrder === 'asc' ? 'desc' : 'asc';
        if (this.state.logOrderBy !== colName) {
            logOrder = 'asc';
        }

        this.setState({
            errorOrderBy: colName,
            errorOrder: logOrder
        }, () => {
            self.refreshErrors();
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

        var filterValue = "Type eq '"+this.props.type+"' and Verbosity eq '0'";
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
            filterValue = "Type eq '"+this.props.type+"' and Verbosity eq '0' and Order eq '0' and " + conds.join(' and ');            
        }

        url += "/events/.search?filter=where$("+filterValue+") "+orderBy+"&startIndex="+startIndex+"&count="+this.state.pageSize;
        return url;
    }

    /**
    * Handle the changes.
    */
    handleChangeValue(e) {
        var self = this;
        self.setState({
            [e.target.name]: e.target.value
        });
    }

    /**
    * Build the error URL.
    */
    getErrorsUrl() {
        var url = Constants.eventSourceUrl;
        var startIndex = this.state.errorPage * this.state.errorPageSize;
        var createdOn = this.state.selectedErrorCreatedOn.format("YYYY-MM-DD");
        var orderBy = "orderby$on(CreatedOn),order("+this.state.errorOrder+")";
        var filterValue = "Type eq '"+this.props.type+"' and Verbosity eq '1' and CreatedOn gt '" + createdOn + "'";
        url += "/events/.search?filter=where$("+filterValue+") "+orderBy+"&startIndex=" + startIndex + "&count=" + this.state.errorPageSize;
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
    * When the page is changed then refresh the ERROR logs.
    */
    handleErrorChangePage(evt, page) {
        var self = this;
        self.setState({
            errorPage: page
        }, () => {
            self.refreshErrors();
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

    /**
    * When the number of records is changed then refresh the ERROR logs.
    */
    handleErrorChangeRowsPage(evt) {
        var self = this;
        self.setState({
            errorPageSize: evt.target.value
        }, () => {
            self.refreshErrors();
        });
    }

    render() {
        var self = this;
        const { t } = this.props;
        var logRows = [];
        if (self.state.data) {
            self.state.data.forEach(function(record) {
                logRows.push((
                    <TableRow hover role="checkbox" key={record.aggregate_id} onClick={() => self.props.history.push('/viewaggregate/' + record.aggregate_id) }>
                        <TableCell>{record.description}</TableCell>
                        <TableCell>{record.client_id}</TableCell>
                        <TableCell>{record.created_on}</TableCell>
                    </TableRow>
                ));
            });
        }

        var errorRows = [];
        if (self.state.errorData) {
            self.state.errorData.forEach(function(record) {
                errorRows.push((
                    <TableRow hover role="checkbox" key={record.id} onClick={() => self.props.history.push('/viewlog/' + record.id) }>
                        <TableCell>{record.code}</TableCell>
                        <TableCell>{record.description}</TableCell>
                        <TableCell>{record.created_on}</TableCell>
                    </TableRow>
                ));
            });
        }

        return (<Grid container spacing={8}>
            <Grid item sm={12} md={6} zeroMinWidth>
                <div className="card">
                    <div className="header">
                        <h4>{self.props.logTitle}</h4>
                    </div>
                    <div className="body" style={{overflowX: "auto"}}>
                        {self.state.loading ? (<CircularProgress />) : (
                            <div>
                                <Table>
                                    <TableHead>
                                        <TableRow>
                                            <TableCell>
                                                <TableSortLabel active={self.state.logOrderBy === 'description' ? true : false} direction={self.state.logOrder} onClick={() => self.handleSortLog('description')}>{t('eventDescription')}</TableSortLabel>                                        
                                            </TableCell>
                                            <TableCell>{t('clientId')}</TableCell>
                                            <TableCell>
                                                <TableSortLabel active={self.state.logOrderBy === 'created_on' ? true : false} direction={self.state.logOrder} onClick={() => self.handleSortLog('created_on')}>{t('createdAfter')}</TableSortLabel>                                        
                                            </TableCell>
                                        </TableRow>
                                    </TableHead>
                                    <TableBody>
                                        <TableRow>
                                            <TableCell>
                                                <form onSubmit={(e) => { e.preventDefault(); self.refreshLogs(); }}>
                                                    <TextField value={this.state.selectedLogEventDescription} name='selectedLogEventDescription' onChange={this.handleChangeValue} placeholder={t('Filter...')}/>
                                                    <IconButton onClick={self.refreshData}><Search /></IconButton>
                                                </form>
                                            </TableCell>
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
                        )}                        
                    </div>
                </div>
            </Grid>
            <Grid item sm={12} md={6} zeroMinWidth>
                <div className="card">
                    <div className="header">
                        <h4>{self.props.errorTitle}</h4>
                    </div>
                    <div className="body" style={{overflow: "hidden"}}>
                        {self.state.errorLoading ? (<CircularProgress />) : (
                            <div>
                                <Table>
                                    <TableHead>
                                        <TableRow>
                                            <TableCell>{t('errorCode')}</TableCell>
                                            <TableCell>{t('errorMessage')}</TableCell>
                                            <TableCell>
                                                <TableSortLabel active={self.state.errorOrderBy === 'created_on' ? true : false} direction={self.state.errorOrder} onClick={() => self.handleSortError('created_on')}>{t('createdAfter')}</TableSortLabel>                                        
                                            </TableCell>
                                        </TableRow>
                                    </TableHead>
                                    <TableBody>
                                        <TableRow>
                                            <TableCell></TableCell>
                                            <TableCell></TableCell>
                                            <TableCell>
                                                <MuiPickersUtilsProvider utils={MomentUtils}>
                                                    <DatePicker value={self.state.selectedErrorCreatedOn} onChange={self.handleErrorCreatedOnChange} />
                                                </MuiPickersUtilsProvider>
                                            </TableCell>
                                        </TableRow>
                                        {errorRows}
                                    </TableBody>
                                </Table>
                                <TablePagination component="div" count={self.state.errorCount} rowsPerPage={self.state.errorPageSize} page={this.state.errorPage} onChangePage={self.handleErrorChangePage} onChangeRowsPerPage={self.handleErrorChangeRowsPage} />
                            </div>
                        )}
                    </div>
                </div>
            </Grid>
        </Grid>);
    }

    componentDidMount() {
        this.refreshLogs();
        this.refreshErrors();
    }
}

export default translate('common', { wait: process && !process.release })(withRouter(LogTables));