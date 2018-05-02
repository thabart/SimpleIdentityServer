import React, { Component } from "react";
import { translate } from 'react-i18next';
import { ClientService } from '../services';
import { GeneralSettingsTab, ScopesTab } from './clientTabs';
import { NavLink, Link } from 'react-router-dom';
import { CircularProgress, IconButton, Grid } from 'material-ui';
import { ChipsSelector } from './common';
import Tabs, { Tab } from 'material-ui/Tabs';
import Save from '@material-ui/icons/Save';
import AppDispatcher from '../appDispatcher';
import Constants from '../constants';

class ViewClient extends Component {
    constructor(props) {
        super(props);
        this.handleTabChange = this.handleTabChange.bind(this);
        this.refreshData = this.refreshData.bind(this);
        this.saveClient = this.saveClient.bind(this);
        this.state = {
            isDisplayed: false,
            id: null,
            type: null,
            isLoading: true,
            client: { },
            tabIndex: 0
        };
    }

    /**
    * Change tab.
    */
    handleTabChange(evt, val) {
        this.setState({
            tabIndex: val
        });
    }

    /**
    * Refresh the client information.
    */
    refreshData() {
        var self = this;
        self.setState({
            isLoading: true
        });
        ClientService.get(self.state.id, self.state.type).then(function(client) {
            self.setState({
                client: client,
                isLoading: false
            });
        }).catch(function(e) {
            self.setState({
                isLoading: false
            });
        });
    }

    /**
    * Save the client.
    */
    saveClient() {        
        var self = this;
        self.setState({
            isLoading: true
        });
        const { t } = self.props;
        ClientService.update(self.state.client, self.state.type).then(function() {
            self.setState({
                isLoading: false
            });
            AppDispatcher.dispatch({
                actionName: Constants.events.DISPLAY_MESSAGE,
                data: t('clientIsUpdated')
            });
        }).catch(function() {
            self.setState({
                isLoading: false
            });
            AppDispatcher.dispatch({
                actionName: Constants.events.DISPLAY_MESSAGE,
                data: t('clientCannotBeUpdated')
            });
        });
    }

    /**
    * Display the view.
    */
    render() {
        var self = this;
        const { t } = self.props;
        if (!self.state.isDisplayed) {
            return (<span></span>);
        }
        
        return (<div className="block">
            <div className="block-header">
                <Grid container>
                    <Grid item md={5} sm={12}>
                        <h4>{t('clientTitle')}</h4>
                        <i>{t('clientShortDescription')}</i>
                    </Grid>
                    <Grid item md={7} sm={12}>                        
                        <ul className="breadcrumb float-md-right">
                            <li className="breadcrumb-item"><NavLink to="/">{t('websiteTitle')}</NavLink></li>
                            <li className="breadcrumb-item"><NavLink to={self.state.type ==="openid" ? "/openidclients" : "/authclients"}>{t('clients')}</NavLink></li>
                            <li className="breadcrumb-item">{t('client')}</li>
                        </ul>
                    </Grid>
                </Grid>
            </div>
            <div className="card">
                <div className="header">
                    <h4 style={{display: "inline-block"}}>{t('clientInformation')}</h4>                    
                    <div style={{float: "right"}}>                        
                        <IconButton onClick={this.saveClient}>
                            <Save />
                        </IconButton>
                    </div>
                </div>
                <div className="body">
                    { self.state.isLoading ? (<CircularProgress />) : (
                        <div>
                            <Tabs indicatorColor="primary" value={self.state.tabIndex} onChange={self.handleTabChange}>
                                <Tab label={t('clientGeneralSettings')} component={Link}  to={"/viewClient/" + self.state.type + "/" + self.state.id} />
                                <Tab label={t('clientScopes')} component={Link}  to={"/viewClient/" + self.state.type + "/" + self.state.id + "/scopes"} />
                            </Tabs>
                            { self.state.tabIndex === 0 && (<GeneralSettingsTab isReadonly={true} type={self.state.type} client={self.state.client} />) }
                            { self.state.tabIndex === 1 && (<ScopesTab allowedScopes={self.state.client.allowed_scopes} type={self.state.type} client={self.state.client} />) }
                        </div>
                    ) }
                </div>
            </div>
        </div>);
    }

    componentDidMount() {
        var self = this;
        var tabIndex = 0;
        if (self.props.match.params.action === 'scopes') {
            tabIndex = 1;
        }

        self.setState({
            id: self.props.match.params.id,
            type: self.props.match.params.type,
            tabIndex: tabIndex,
            isDisplayed: true
        }, function() {
            self.refreshData();
        });
    }
}

export default translate('common', { wait: process && !process.release })(ViewClient);