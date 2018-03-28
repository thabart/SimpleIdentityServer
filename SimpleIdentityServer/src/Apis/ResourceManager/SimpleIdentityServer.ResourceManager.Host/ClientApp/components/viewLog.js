import React, { Component } from "react";
import { translate } from 'react-i18next';
import Constants from '../constants';
import $ from 'jquery';
import moment from 'moment';

class ViewLog extends Component {
    constructor(props) {
        super(props);
        this.refreshData = this.refreshData.bind(this);
        this.state = {
            id: null,
            aggregateId: null,
            description: null,
            createdOn: null,
            payload: null
        };
    }

    refreshData() {
        var self = this;
        var href = Constants.eventSourceUrl + "/events/" + self.props.match.params.id;
        $.get(href).done(function (result) {
            self.setState({
                id: result.id,
                aggregateId: result.aggregate_id,
                description: result.description,
                createdOn: moment(result.createdOn).format('LLL'),
                payload: result.payload
            });
        }).fail(function () {

        });

    }

    render() {
        var self = this;
        const { t } = self.props;
        return (<div className="block">
            <div className="block-header">
                <h4>{t('logTitle')}</h4>
                <i>{t('logShortDescription')}</i>
            </div>
            <div className="container-fluid">
                <div className="row">
                    <div className="col-md-12">
                        <div className="card">
                            <div className="header"><h4>{t('logDescription')}</h4></div>
                            <div className="body">
                                <div className="form-group">
                                    <label>{t('id')}</label>
                                    <input type="text" className="form-control" value={this.state.id} />
                                </div>
                                <div className="form-group">
                                    <label>{t('aggregateId')}</label>
                                    <input type="text" className="form-control" value={this.state.aggregateId} />
                                </div>
                                <div className="form-group">
                                    <label>{t('description')}</label>
                                    <input type="text" className="form-control" value={this.state.description} />
                                </div>
                                <div className="form-group">
                                    <label>{t('createdOn')}</label>
                                    <input type="text" className="form-control" value={this.state.createdOn} />
                                </div>
                                <div className="form-group">
                                    <label>{t('payload')}</label>
                                    <textarea className="form-control" value={this.state.payload} />
                                </div>
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

export default translate('common', { wait: process && !process.release })(ViewLog);