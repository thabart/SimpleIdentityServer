import React, { Component } from "react";
import { translate } from 'react-i18next';
import { withRouter, NavLink } from 'react-router-dom';
import Constants from '../constants';
import $ from 'jquery';
import moment from 'moment';
import Table, { TableBody, TableCell, TableHead, TableRow, TableFooter, TablePagination, TableSortLabel } from 'material-ui/Table';
import { CircularProgress, Tooltip, Grid  } from 'material-ui';

class ViewAggregate extends Component {
    constructor(props) {
        super(props);
        this.refreshData = this.refreshData.bind(this);
        this.state = {
            data: [],
            loading: true
        };
    }

    /**
    * Get well formated date.
    */
    getDate(d) {
        return moment(d).format('LLL');
    }

    /**
    * Refresh the data.
    */
    refreshData() {
        var self = this;
        self.setState({
            loading: true
        });
        var href = Constants.eventSourceUrl + "/events/.search?filter=where$(AggregateId eq '" + self.props.match.params.id + "') orderby$on(Order),order(asc)";
        $.get(href).done(function (result) {
            var data = [];
            if (result.content && result.content.length > 0) {
                result.content.forEach(function (rec) {
                    data.push({
                        description: rec.Description,
                        id : rec.Id,
                        created_on: self.getDate(rec.CreatedOn),
                        payload: rec.Payload
                    })
                });
            }

            self.setState({
                data: data,
                loading: false
            });
        }).fail(function () {
            self.setState({
                loading: false
            });
        });
    }

    render() {
        var self = this;
        const { t } = self.props;
        var rows = [];
        if (self.state.data) {
            self.state.data.forEach(function(record) {                
                rows.push((
                    <TableRow hover role="checkbox" key={record.id} onClick={() => self.props.history.push('/viewlog/' + record.id) }>
                        <TableCell>{record.description}</TableCell>
                        <TableCell>{record.created_on}</TableCell>
                        <TableCell style={{ overflow: "hidden", textOverflow: "ellipsis", whiteSpace: "nowrap", maxWidth: "200px" }}>
                            <span title={record.payload}>{record.payload}</span>
                        </TableCell>
                    </TableRow>
                ));
            });
        }

        return (<div className="block">
            <div className="block-header">
                <Grid container>
                    <Grid item md={5} sm={12}>
                        <h4>{t('logAggregateTitle')}</h4>
                        <i>{t('logAggregateShortDescription')}</i>
                    </Grid>
                    <Grid item md={7} sm={12}>                        
                        <ul className="breadcrumb float-md-right">
                            <li className="breadcrumb-item"><NavLink to="/">{t('websiteTitle')}</NavLink></li>
                            <li className="breadcrumb-item"><NavLink to="/logs">{t('logs')}</NavLink></li>
                            <li className="breadcrumb-item">{t('logAggregate')}</li>
                        </ul>
                    </Grid>
                </Grid>
            </div>
            <div className="container-fluid">
                <div className="row">
                    <div className="col-md-12">
                        <div className="card">
                            <div className="header"><h4>{t('aggregateLogsTitle')}</h4></div>
                            <div className="body" style={{ overflow: "hidden" }}>
                                {self.state.loading ? (<CircularProgress />) : (
                                    <Table>
                                        <TableHead>
                                            <TableRow>
                                                <TableCell>{t('event')}</TableCell>
                                                <TableCell>{t('createdOn')}</TableCell>
                                                <TableCell>{t('payload')}</TableCell>
                                            </TableRow>
                                        </TableHead>
                                        <TableBody>
                                            {rows}
                                        </TableBody>
                                    </Table>
                                )}                              
                            </div>
                        </div>
                    </div>
                </div>
            </div>
        </div>);
    }

    componentDidMount() {
        this.refreshData();
    }
}

export default translate('common', { wait: process && !process.release })(withRouter(ViewAggregate));