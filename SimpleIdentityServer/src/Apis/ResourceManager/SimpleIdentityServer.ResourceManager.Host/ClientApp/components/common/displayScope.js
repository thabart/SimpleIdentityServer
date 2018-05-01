import React, { Component } from "react";
import { translate } from 'react-i18next';
import { withStyles } from 'material-ui/styles';
import { SessionService } from '../../services';
import $ from 'jquery';
import { CircularProgress, IconButton, Select, MenuItem, Checkbox, Typography, Grid } from 'material-ui';
import Input, { InputLabel } from 'material-ui/Input';
import { FormControl, FormHelperText } from 'material-ui/Form';
import { ChipsSelector } from '../common';
import Tabs, { Tab } from 'material-ui/Tabs';
import Save from '@material-ui/icons/Save';
import AppDispatcher from '../../appDispatcher';
import Constants from '../../constants';
import { SessionStore } from '../../stores';

const styles = theme => ({
  margin: {
    margin: theme.spacing.unit,
  }
});

class DisplayScope extends Component {
    constructor(props) {
        super(props);
        this.saveScope = this.saveScope.bind(this);
        this.handleChangeProperty = this.handleChangeProperty.bind(this);
        this.handleToggleProperty = this.handleToggleProperty.bind(this);
        this.refreshData = this.refreshData.bind(this);
        this.state = {
            scope: props.scope,
            type: props.type,
            claims: [],
            chipsSelectorOpts: {
                type: 'select',
                values: []
            }
        };
    }
    /**
    * Save the scope.
    */
    saveScope() {
        if (this.props.onSave) {
            this.props.onSave(this.state.scope);
        }
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
    * Display the claims.
    */
    refreshData() {
        var self = this;
        const { t } = self.props;
        var profile = SessionStore.getSession();
        var type = self.state.type;
        var url = '';
        switch(type) {
            case "openid":
                url = profile.openid_url;
            break;
            case "auth":
                url = profile.auth_url;
            break;
            default:
                return;
        }

        self.setState({
            isLoading: true
        });
        $.get(url).then(function(result) {
            var claims = result['claims_supported'];
            var opts = self.state.chipsSelectorOpts;
            if (claims) {
                claims.forEach(function(claim) {
                    opts.values.push({ key: claim, label: claim });
                });
            }

            self.setState({
                isLoading: false,
                chipsSelectorOpts: opts
            });
        }).fail(function() {
            AppDispatcher.dispatch({
                data: t('claimsCannotBeRetrieved'),
                actionName: Constants.events.DISPLAY_MESSAGE
            });
            self.setState({
                isLoading: false,
                chipsSelectorOpts: {
                    type: 'select',
                    values: []
                }
            });
        });
    }

    /**
    * Display the view.
    */
    render() {
        var self = this;
        const { t, classes } = self.props;
        return (
                <Grid container spacing={40}>
                    <Grid item sm={12} md={6} className={classes.grid}>
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
                    </Grid>
                    <Grid item sm={12} md={6} className={classes.grid}>
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
                            <Typography><Checkbox color="primary" checked={self.state.scope.is_openid_scope} onChange={() => { self.handleToggleProperty('is_openid_scope'); }}/> {t('isOpenidScope')}</Typography >
                            <Typography>{t('isOpenidScopeDescription')}</Typography>
                        </div>
                        { self.state.scope.type === 1 && self.state.chipsSelectorOpts.values.length > 0 && ( <ChipsSelector label={t('addScopeClaims')} properties={self.state.scope.claims} input={self.state.chipsSelectorOpts} />) }                        
                    </Grid>
                </Grid>
        );
    }

    componentDidMount() {
        var self = this;
        SessionStore.addChangeListener(function() {
            self.refreshData();
        });
        self.refreshData();
    }
}

export default translate('common', { wait: process && !process.release })(withStyles(styles)(DisplayScope));