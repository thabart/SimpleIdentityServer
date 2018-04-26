import React, { Component } from "react";
import { translate } from 'react-i18next';
import { ResourcesTab, HierarchicalResourcesTab } from './resourcesTabs';
import { withRouter, Link } from 'react-router-dom';
import Tabs, { Tab } from 'material-ui/Tabs';

class Resources extends Component {
    constructor(props) {
        super(props);
        this.handleChangeTab = this.handleChangeTab.bind(this);
        this.state = {
            tabId: 0
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
        return (<div className="block">
            <div className="block-header">
                <h4>{t('resourcesTitle')}</h4>
                <i>{t('resourcesShortDescription')}</i>
            </div>
            <div className="card">
                <div className="body">
                                <Tabs value={self.state.tabId} onChange={self.handleChangeTab}>
                                    <Tab label={t('resources')} component={Link}  to="/resources" />
                                    <Tab label={t('hierarchicalResources')} component={Link}  to="/resources/hierarchy" />
                                </Tabs>
                </div>
            </div>
            { this.state.tabId === 0 && (<ResourcesTab />) }
            { this.state.tabId === 1 && (<HierarchicalResourcesTab />) }
        </div>);
    }

    componentDidMount() {        
        var self = this;
        var action = self.props.match.params.action;
        if (action === 'hierarchy') {
            self.setState({
                tabId: 1
            });
        }
    }
}

export default translate('common', { wait: process && !process.release })(Resources);