import React, { Component } from "react";
import { translate } from 'react-i18next';
import { ChartsTab, LogsTab } from './openIdTabs';
import { withRouter } from 'react-router-dom';

class OpenIdTab extends Component {
    constructor(props) {
        super(props);
        this.navigate = this.navigate.bind(this);
        this.state = {
            tabName : null
        };
    }

    navigate(e, name) {
        e.preventDefault();
        this.setState({
            tabName: name
        });
        var action = this.props.match.params.action;
        this.props.history.push('/logs/' + action + '/' + name);
    }

    render() {
        var self = this;
        const { t } = self.props;
        return (<div>
            <h4>{t('openIdLogsTitle')}</h4>
            <ul className="nav nav-tabs">
                <li className="nav-item">
                    <a href="#" className="nav-link" onClick={(e) => self.navigate(e, "logs")}>{t('logs')}</a>
                </li>
                <li className="nav-item">
                    <a href="#" className="nav-link" onClick={(e) => self.navigate(e, "charts")}>{t('charts')}</a>
                </li>
            </ul>
            <div className="tab-content">
                {this.state.tabName === 'logs' && (<ChartsTab />)}
                {this.state.tabName === 'charts' && (<ChartsTab />)}
            </div>
        </div>);
    }

    getSubAction() {
        var self = this;
        var subAction = self.props.match.params.subaction;
        if (!subAction || (subAction !== 'logs' && subAction !== 'charts')) {
            subAction = 'logs';
        }

        return subAction;
    }

    componentDidMount() {
        this.setState({
            tabName: this.getSubAction()
        });
    }
}

export default translate('common', { wait: process && !process.release })(withRouter(OpenIdTab));