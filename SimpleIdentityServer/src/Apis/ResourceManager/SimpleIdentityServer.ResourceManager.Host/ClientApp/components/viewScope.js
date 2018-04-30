import React, { Component } from "react";
import { translate } from 'react-i18next';
import { ScopeService } from '../services';
import { withStyles } from 'material-ui/styles';
import { CircularProgress, IconButton, Select, MenuItem, Checkbox, Typography } from 'material-ui';
import Input, { InputLabel } from 'material-ui/Input';
import { DisplayScope } from './common';
import Tabs, { Tab } from 'material-ui/Tabs';
import Save from '@material-ui/icons/Save';

class ViewScope extends Component {
    constructor(props) {
        super(props);
        this.saveScope = this.saveScope.bind(this);
        this.refreshData = this.refreshData.bind(this);
        this.state = {
            isLoading: true,
            id: null,
            type: null,
            scope: {}
        };
    }
    /**
    * Save the scope.
    */
    saveScope() {
        
    }

    /**
    * Display the scope.
    */
    refreshData() {
        var self = this;
        self.setState({
            isLoading: true
        });
        ScopeService.get(self.state.id, self.state.type).then(function(scope) {
            self.setState({
                isLoading: false,
                scope: scope
            });
        }).catch(function() {
            self.setState({
                isLoading: false
            });
        });
    }

    /**
    * Display the view.
    */
    render() {
        var self = this;
        const { t, classes } = self.props;
        return (<div className="block">
            <div className="block-header">
                <h4>{t('scopeTitle')}</h4>
                <i>{t('scopeShortDescription')}</i>
            </div>
            <div className="card">
                <div className="header">
                    <h4 style={{display: "inline-block"}}>{t('scopeInformation')}</h4>                    
                    <div style={{float: "right"}}>                        
                        <IconButton onClick={this.saveScope}>
                            <Save />
                        </IconButton>
                    </div>
                </div>
                <div className="body">
                    { self.state.isLoading ? (<CircularProgress />) : (
                        <DisplayScope type={self.state.type} scope={self.state.scope} />
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

export default translate('common', { wait: process && !process.release })(ViewScope);