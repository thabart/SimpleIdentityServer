import React, { Component } from "react";
import { translate } from 'react-i18next';
import { CircularProgress, Select, MenuItem, Checkbox, Button, Paper, Typography, Chip, Grid } from 'material-ui';
import { ChipsSelector } from '../common';
import { withStyles } from 'material-ui/styles';
import Input, { InputLabel } from 'material-ui/Input';
import { FormControl, FormHelperText } from 'material-ui/Form';
import $ from 'jquery';
import Tabs, { Tab } from 'material-ui/Tabs';
import AppDispatcher from '../../appDispatcher';
import Constants from '../../constants';
import { SessionStore } from '../../stores';

const styles = theme => ({
  margin: {
    margin: theme.spacing.unit,
  },
  paper: {
    padding: theme.spacing.unit,
    marginTop: theme.spacing.unit
  }
});

class GeneralSettingsTab extends Component {
	constructor(props) {
		super(props);
        this.refreshData = this.refreshData.bind(this);
		this.handleToggleValue = this.handleToggleValue.bind(this);
		this.handleChangeProperty = this.handleChangeProperty.bind(this);
		this.handleChangeClientSecret = this.handleChangeClientSecret.bind(this);
        this.handleTabChange = this.handleTabChange.bind(this);
		this.state = {
            isLoading: true,
			client: props.client,
			isAdvSettingsDisplayed: false,
			isClearClientSecret: false,
            tabIndex: 0,
            grantTypeSelectorOpts: {},
            jsonWebTokenSignature: [],
            tokenEdpAuthMethods: []
		};
	}

    /**
    * Refresh the data.
    */
    refreshData() {        
        var self = this;
        self.setState({
            isLoading: true
        });
        const { t } = self.props;
        var profile = SessionStore.getSession();
        if (!profile.openid_url) {
            self.setState({
                isLoading: false
            });
            return;
        }


        var type = self.props.type;
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

        $.get(url).then(function(result) {
            var grantTypesSupported = [],
                jsonWebTokenSignature = [],
                tokenEdpAuthMethods = [];
            if (result['grant_types_supported']) {
                result['grant_types_supported'].forEach(function(authMethod) {
                    grantTypesSupported.push({
                        key: authMethod,
                        label: authMethod
                    })
                });
            }

            if (result['id_token_signing_alg_values_supported']) {
                jsonWebTokenSignature = result['id_token_signing_alg_values_supported'];
            }

            if (result['token_endpoint_auth_methods_supported']) {
                tokenEdpAuthMethods = result['token_endpoint_auth_methods_supported'];
            }

            self.setState({
                isLoading: false,
                grantTypeSelectorOpts: {
                    type: 'select',
                    values: grantTypesSupported
                },
                jsonWebTokenSignature: jsonWebTokenSignature,
                tokenEdpAuthMethods: tokenEdpAuthMethods
            });
        }).fail(function() {
            self.setState({
                isLoading: false,
                grantTypeSelectorOpts: {
                    type: 'select',
                    values: []
                },
                jsonWebTokenSignature: [],
                tokenEdpAuthMethods: []
            });
            AppDispatcher.dispatch({
                actionName: Constants.events.DISPLAY_MESSAGE,
                data: t('openidConfigurationCannotBeRetrieved')
            });
        });
    }

	/**
	* Toggle a value.
	*/
	handleToggleValue(name) {
		var self = this;
		self.setState({
			[name]: !self.state[name]
		});
	}

	/**
	* Change the property.
	*/
	handleChangeProperty(e) {
		var self = this;
		var client = self.state.client;
		client[e.target.name] = e.target.value;
		self.setState({
			client: client
		});
	}

	/**
	* Change client secret.
	*/
	handleChangeClientSecret(e) {
		var self = this;
		var client = self.state.client;
		client.secrets[0].value = e.target.value;
		self.setState({
			client: client
		});
	}

    /**
    * Change tab.
    */
    handleTabChange(e, val) {        
        this.setState({
            tabIndex: val
        });
    }

    /**
    * Display the view.
    */
    render() {
        var self = this;
        const { t, classes } = self.props;
        var grantTypes = [];
        if (self.state.grantTypes) {
            self.state.grantTypes.forEach(function(grantType) {
                grantTypes.push((<Chip label={t(grantType.label)} key={grantType.key} className={classes.margin} />));
            });
        }

        if (self.state.isLoading) {
            return (<CircularProgress />);
        }

        var tokenAuthMethods = [],
            jsonWebTokenSignature = [];
        if (self.state.tokenEdpAuthMethods) {
            self.state.tokenEdpAuthMethods.forEach(function(tokenAuthMethod) {
                tokenAuthMethods.push((<MenuItem key={tokenAuthMethod} value={tokenAuthMethod}>{tokenAuthMethod}</MenuItem>));
            });
        }

        if (self.state.jsonWebTokenSignature) {
            self.state.jsonWebTokenSignature.forEach(function(jsonWebTokenSig) {
                jsonWebTokenSignature.push((<MenuItem key={jsonWebTokenSig} value={jsonWebTokenSig}>{jsonWebTokenSig}</MenuItem>));
            });
        }
        
        return (<Grid container spacing={40}>
                    <Grid item md={6} sm={12}>
                        {/* Client name */}
                        <FormControl fullWidth={true} className={classes.margin}>
                            <InputLabel htmlFor="clientName">{t('clientName')}</InputLabel>
                            <Input id="clientName" value={self.state.client.client_name} name="client_name" onChange={self.handleChangeProperty}  />
                        </FormControl>
                        {/* Client ID */}
                        <FormControl fullWidth={true} className={classes.margin} disabled={self.props.isReadonly}>
                            <InputLabel htmlFor="clientId">{t('clientId')}</InputLabel>
                            <Input id="clientId" value={self.state.client.client_id} name="client_id" onChange={self.handleChangeProperty}  />
                        </FormControl>
                        {/* Client secret */}
                        <FormControl fullWidth={true} className={classes.margin}>
                            <InputLabel htmlFor="clientSecret">{t('clientSecret')}</InputLabel>
                            <Input id="clientSecret" type={self.state.isClearClientSecret ? "text" : "password"} value={self.state.client.secrets[0].value} onChange={self.handleChangeClientSecret} />                                
                            <FormHelperText>
                                <Checkbox color="primary" onChange={() => self.handleToggleValue('isClearClientSecret')} /><span>{t('revealClientSecret')}</span>
                            </FormHelperText>
                        </FormControl>
                        {/* Client logo */}
                        <FormControl fullWidth={true} className={classes.margin}>
                            <InputLabel htmlFor="clientLogo">{t('clientLogo')}</InputLabel>
                            <Input id="clientLogo" value={self.state.client.logo_uri} name="logo_uri" onChange={self.handleChangeProperty}  />
                            <FormHelperText>{t('clientLogoDescription')}</FormHelperText>
                        </FormControl>
                    </Grid>
                    <Grid item md={6} sm={12}>
                        {/* Application type */}
                        <FormControl fullWidth={true} className={classes.margin}>
                            <InputLabel htmlFor="clientApplicationType">{t('clientApplicationType')}</InputLabel>                                                                                    
                            <Select value={self.state.client.application_type} fullWidth={true} onChange={self.handleChangeProperty} name="application_type">
                                <MenuItem value="native">{t('native')}</MenuItem>
                                <MenuItem value="web">{t('web')}</MenuItem>
                            </Select>
                            <FormHelperText>{t('clientApplicationTypeDescription')}</FormHelperText>
                        </FormControl>
                        {/* Allowed callback urls */}
                        <div className={classes.margin}>
                            <ChipsSelector label={t('clientAllowedCallbackUrls')} properties={self.state.client.redirect_uris} />
                            <FormHelperText>{t('clientAllowedCallbackUrlsDescription')}</FormHelperText>
                        </div>
                        {/* Allowed logout urls */}
                        <div className={classes.margin}>
                            <ChipsSelector label={t('clientAllowedLogoutUrls')} properties={self.state.client.post_logout_redirect_uris} />
                            <FormHelperText>{t('clientAllowedLogoutUrlsDescription')}</FormHelperText>
                        </div>
                    </Grid>
                    <Grid item sm={12}>
                    <Button variant="raised" color="primary" onClick={() => self.handleToggleValue('isAdvSettingsDisplayed')}>{t('showAdvancedSettings')}</Button>
                    {self.state.isAdvSettingsDisplayed && (
                        <Paper className={classes.paper} >
                            <Tabs indicatorColor="primary" value={self.state.tabIndex} onChange={self.handleTabChange}>
                                <Tab label={t('clientGrantTypes')} />
                                <Tab label={t('clientOAuth')} />
                            </Tabs>
                            {self.state.tabIndex === 0 && (
                                <div>
                                    <ChipsSelector properties={self.state.client.grant_types} label={t('chooseClientGrantTypes')} input={self.state.grantTypeSelectorOpts} />
                                    <Typography variant="caption" gutterBottom>{t('chooseClientGrantTypesDescription')}</Typography>
                                </div>
                            )}
                            {self.state.tabIndex === 1 && (
                                <div>
                                    <FormControl fullWidth={true} className={classes.margin}>
                                        <InputLabel htmlFor="clientApplicationType">{t('jsonWebTokenSignatureAlg')}</InputLabel>   
                                        <Select value={this.state.client.id_token_signed_response_alg} name="id_token_signed_response_alg" onChange={this.handleChangeProperty}>
                                            {jsonWebTokenSignature}
                                        </Select>
                                        <FormHelperText>{t('jsonWebTokenSignatureAlgDescription')}</FormHelperText>
                                    </FormControl>
                                    <FormControl fullWidth={true} className={classes.margin}>
                                        <InputLabel htmlFor="clientApplicationType">{t('tokenAuthMethod')}</InputLabel>   
                                        <Select value={this.state.client.token_endpoint_auth_method} name="token_endpoint_auth_method" onChange={this.handleChangeProperty}>
                                            {tokenAuthMethods}
                                        </Select>
                                        <FormHelperText>{t('tokenAuthMethodDescription')}</FormHelperText>
                                    </FormControl>

                                </div>
                            )}
                        </Paper>
                    )}
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

export default translate('common', { wait: process && !process.release })(withStyles(styles)(GeneralSettingsTab));