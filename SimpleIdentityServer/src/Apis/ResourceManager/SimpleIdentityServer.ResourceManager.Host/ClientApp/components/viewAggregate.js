import React, { Component } from "react";
import { translate } from 'react-i18next';
import { withRouter } from 'react-router-dom';
import ReactTable from 'react-table'
import Constants from '../constants';
import $ from 'jquery';
import moment from 'moment';

const columns = [
    { Header: 'Event', accessor: 'description' },
    { Header: 'Created on', accessor: 'created_on' },
    {
        Header: 'Payload', accessor: 'payload', Cell: function (row) {
            return (<span title={row.original.payload}>{row.original.payload}</span>);
        }
    }
];

class ViewAggregate extends Component {
    constructor(props) {
        super(props);
        this.fetchData = this.fetchData.bind(this);
        this.state = {
            data: [],
            loading: true,
            pages: null
        };
    }

    getDate(d) {
        return moment(d).format('LLL');
    }

    fetchData() {
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

            var pages = Math.round((result.totalResults + result.itemsPerPage - 1) / result.itemsPerPage);
            self.setState({
                data: data,
                loading: false,
                pages: pages
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
        return (<div className="block">
            <div className="block-header">
                <h4>{t('logAggregateTitle')}</h4>
                <i>{t('logAggregateShortDescription')}</i>
            </div>
            <div className="container-fluid">
                <div className="row">
                    <div className="col-md-12">
                        <div className="card">
                            <div className="header"><h4>{t('aggregateLogsTitle')}</h4></div>
                            <div className="body">
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
                                    className="-striped -highlight"
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
                </div>
            </div>
        </div>);
    }
}

export default translate('common', { wait: process && !process.release })(withRouter(ViewAggregate));