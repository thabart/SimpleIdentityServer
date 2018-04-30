import React, { Component } from "react";
import { translate } from 'react-i18next';
import { ClientService } from '../services';
import { GeneralSettingsTab } from './clientTabs';
import { CircularProgress, IconButton } from 'material-ui';
import { ChipsSelector } from './common';
import Tabs, { Tab } from 'material-ui/Tabs';
import Save from '@material-ui/icons/Save';

class ViewClient extends Component {
    constructor(props) {
        super(props);
        this.handleTabChange = this.handleTabChange.bind(this);
        this.refreshData = this.refreshData.bind(this);
        this.saveClient = this.saveClient.bind(this);
        this.state = {
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
            console.log(client);
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

    }

    /**
    * Display the view.
    */
    render() {
        var self = this;
        const { t } = self.props;
        return (<div className="block">
            <div className="block-header">
                <h4>{t('clientTitle')}</h4>
                <i>{t('clientShortDescription')}</i>
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
                            <Tabs value={self.state.tabIndex} onChange={self.handleTabChange}>
                                <Tab label={t('clientGeneralSettings')} />
                                <Tab label={t('clientScopes')} />
                            </Tabs>
                            { self.state.tabIndex === 0 && (<GeneralSettingsTab client={self.state.client} />) }
                        </div>
                    ) }
                </div>
            </div>
        </div>);
    }

    componentDidMount() {
        var self = this;
        self.setState({
            id: self.props.match.params.id,
            type: self.props.match.params.type
        }, function() {
            self.refreshData();
        });
    }
}

export default translate('common', { wait: process && !process.release })(ViewClient);