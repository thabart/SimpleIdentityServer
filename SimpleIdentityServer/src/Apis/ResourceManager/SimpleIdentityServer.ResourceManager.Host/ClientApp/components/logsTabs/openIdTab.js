import React, { Component } from "react";
import { translate } from 'react-i18next';
import { ChartsTab } from './openIdTabs';
import { LogTables } from '../common';
import { withRouter, Link } from 'react-router-dom';
import Tabs, { Tab } from 'material-ui/Tabs';

class OpenIdTab extends Component {
    constructor(props) {
        super(props);
        this.handleChangeTab = this.handleChangeTab.bind(this);
        this.state = {
            tabId : 0
        };
    }

    handleChangeTab(e, v) {
        var self = this;
        self.setState({
            tabId: v
        });
    }

    render() {
        var self = this;
        const { t } = self.props;
        return (
            <div>
                <div className="card">
                    <div className="body">
                        <Tabs indicatorColor="primary" value={self.state.tabId} onChange={self.handleChangeTab}>
                            <Tab label={t('logs')} component={Link}  to="/logs/openid/logs" />
                            <Tab label={t('charts')} component={Link}  to="/logs/openid/charts" />
                        </Tabs>
                    </div>
                </div>
                <div>
                    {this.state.tabId === 0 && (<LogTables logTitle={t('openidLogsTitle')} errorTitle={t('openidErrorsTitle')} type='openid' />)}
                    {this.state.tabId === 1 && (<ChartsTab />)}
                </div>
            </div>);
    }

    getTabId() {
        var self = this;
        var subAction = self.props.match.params.subaction;
        if (!subAction || (subAction !== 'logs' && subAction !== 'charts')) {
            subAction = 'logs';
        }

        if(subAction === 'logs') {
            return 0;
        }

        return 1;
    }

    componentDidMount() {
        this.setState({
            tabId: this.getTabId()
        });
    }
}

export default translate('common', { wait: process && !process.release })(withRouter(OpenIdTab));