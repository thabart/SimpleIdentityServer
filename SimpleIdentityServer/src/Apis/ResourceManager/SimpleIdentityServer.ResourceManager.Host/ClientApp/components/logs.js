import React, { Component } from "react";
import { translate } from 'react-i18next';
import { NavLink, Link } from 'react-router-dom';
import { ScimTab, OpenIdTab, AuthorizationTab } from './logsTabs';
import { withRouter } from 'react-router-dom';
import { Grid } from 'material-ui';
import Tabs, { Tab } from 'material-ui/Tabs';

class Logs extends Component {
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
                <Grid container>
                    <Grid item md={5} sm={12}>
                        <h4>{t('logsTitle')}</h4>
                        <i>{t('logsShortDescription')}</i>
                    </Grid>
                    <Grid item md={7} sm={12}>                        
                        <ul className="breadcrumb float-md-right">
                            <li className="breadcrumb-item"><NavLink to="/">{t('websiteTitle')}</NavLink></li>
                            <li className="breadcrumb-item">{t('logs')}</li>
                        </ul>
                    </Grid>
                </Grid>
            </div>
            <div style={{padding: "0px 5px 0px 5px"}}>
                <div className="card">
                    <div className="body">
                        <Tabs indicatorColor="primary" value={self.state.tabId} onChange={self.handleChangeTab}>
                            <Tab label={t('openid')} component={Link}  to="/logs/openid" />
                            <Tab label={t('scim')} component={Link}  to="/logs/scim" />
                            <Tab label={t('authorization')} component={Link}  to="/logs/authorization" />
                        </Tabs>
                    </div>
                </div>
                <div>
                    {self.state.tabId === 0 && (<OpenIdTab />)}
                    {self.state.tabId === 1 && (<ScimTab />)}
                    {self.state.tabId === 2 && (<AuthorizationTab />)}
                </div>
            </div>
        </div>);
    }

    componentDidMount() {
        var self = this;
        var action = self.props.match.params.action;
        if (!action || (action !== 'openid' && action !== 'authorization' && action !== 'scim')) {
            action = 'openid';
        }

        var tabId = 0;
        switch(action) {
            case "scim":
                tabId = 1;
            break;
            case "authorization":
                tabId = 2;
            break;
        }
        console.log(tabId);
        self.setState({
            tabId: tabId
        });
    }
}

export default translate('common', { wait: process && !process.release })(withRouter(Logs));