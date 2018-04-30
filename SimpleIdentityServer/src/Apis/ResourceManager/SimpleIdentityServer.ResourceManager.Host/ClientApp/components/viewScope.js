import React, { Component } from "react";
import { translate } from 'react-i18next';
import { ScopeService } from '../services';
import { GeneralSettingsTab } from './clientTabs';
import { withStyles } from 'material-ui/styles';
import { CircularProgress, IconButton, Select, MenuItem, Checkbox, Typography } from 'material-ui';
import Input, { InputLabel } from 'material-ui/Input';
import { FormControl, FormHelperText } from 'material-ui/Form';
import { ChipsSelector } from './common';
import Tabs, { Tab } from 'material-ui/Tabs';
import Save from '@material-ui/icons/Save';

const styles = theme => ({
  margin: {
    margin: theme.spacing.unit,
  }
});

class ViewScope extends Component {
    constructor(props) {
        super(props);
        this.saveScope = this.saveScope.bind(this);
        this.refreshData = this.refreshData.bind(this);
        this.handleChangeProperty = this.handleChangeProperty.bind(this);
        this.handleToggleProperty = this.handleToggleProperty.bind(this);
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
        console.log("*** SAVE THE SCOPE ***");
        console.log(this.state.scope);
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
    * Change the property.
    */
    handleChangeProperty(e) {
        var self = this;
        var scope = self.state.scope;
        scope[e.target.name] = e.target.value;
        self.setState({
            scope: scope
        });
    }

    /**
    * Toggle a property.
    */
    handleToggleProperty(name) {
        var self = this;
        var scope = self.state.scope;
        scope[name] = !scope[name];
        self.setState({
            scope: scope
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
                        <div>
                            {/* Name */}
                            <FormControl fullWidth={true} className={classes.margin}>
                                <InputLabel>{t('scopeName')}</InputLabel>
                                <Input value={self.state.scope.name} name="name" onChange={self.handleChangeProperty}  />
                                <FormHelperText>{t('scopeNameDescription')}</FormHelperText>
                            </FormControl>
                            {/* Description */}
                            <FormControl fullWidth={true} className={classes.margin}>
                                <InputLabel>{t('scopeDescription')}</InputLabel>
                                <Input value={self.state.scope.description} name="description" onChange={self.handleChangeProperty}  />
                                <FormHelperText>{t('scopeDescriptionDescription')}</FormHelperText>
                            </FormControl>
                            {/* Type */}
                            <FormControl fullWidth={true} className={classes.margin}>                                
                                <InputLabel>{t('scopeType')}</InputLabel>
                                <Select value={self.state.scope.type} onChange={self.handleChangeProperty} name="type">
                                    <MenuItem value={0}>{t('apiScope')}</MenuItem>
                                    <MenuItem value={1}>{t('resourceOwnerScope')}</MenuItem>
                                </Select>
                                <FormHelperText>{t('scopeTypeDescription')}</FormHelperText>
                            </FormControl>
                            {/* Openid scope */}
                            <div>
                                <Typography><Checkbox checked={self.state.scope.is_openid_scope} onChange={() => self.handleToggleProperty('is_openid_scope')}/> {t('isOpenidScope')}</Typography >
                                <Typography>{t('isOpenidScopeDescription')}</Typography>
                            </div>
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

export default translate('common', { wait: process && !process.release })(withStyles(styles)(ViewScope));