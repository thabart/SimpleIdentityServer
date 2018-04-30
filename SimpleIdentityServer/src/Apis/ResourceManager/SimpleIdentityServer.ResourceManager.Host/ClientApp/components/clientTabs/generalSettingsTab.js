import React, { Component } from "react";
import { translate } from 'react-i18next';
import { CircularProgress, Select, MenuItem, Checkbox, Button, Paper, Typography, Chip } from 'material-ui';
import { ChipsSelector } from '../common';
import { withStyles } from 'material-ui/styles';
import Input, { InputLabel } from 'material-ui/Input';
import { FormControl, FormHelperText } from 'material-ui/Form';
import Tabs, { Tab } from 'material-ui/Tabs';

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
		this.handleToggleValue = this.handleToggleValue.bind(this);
		this.handleChangeProperty = this.handleChangeProperty.bind(this);
		this.handleChangeClientSecret = this.handleChangeClientSecret.bind(this);
        this.handleTabChange = this.handleTabChange.bind(this);
		this.state = {
			client: props.client,
            grantTypeSelectorOpts: {
                type: 'select',
                values: [                    
                    { key: "authorization_code", label: "authorizationCode" },
                    { key: "implicit", label: "implicit" },
                    { key: "refresh_token", label: "refreshToken" },
                    { key: "client_credentials", label: "clientCredentials" },
                    { key: "password", label: "password" }
                ]
            },
			isAdvSettingsDisplayed: false,
			isClearClientSecret: false,
            tabIndex: 0
		};
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

        return (<div>
                	{/* Client name */}
                    <FormControl fullWidth={true} className={classes.margin}>
                    	<InputLabel htmlFor="clientName">{t('clientName')}</InputLabel>
                        <Input id="clientName" value={self.state.client.client_name} name="client_name" onChange={self.handleChangeProperty}  />
                    </FormControl>
                    {/* Client ID */}
                    <FormControl fullWidth={true} className={classes.margin}>
                    	<InputLabel htmlFor="clientId">{t('clientId')}</InputLabel>
                        <Input id="clientId" value={self.state.client.client_id} name="client_id" onChange={self.handleChangeProperty}  />
                    </FormControl>
                    {/* Client secret */}
                    <FormControl fullWidth={true} className={classes.margin}>
                    	<InputLabel htmlFor="clientSecret">{t('clientSecret')}</InputLabel>
                        <Input id="clientSecret" type={self.state.isClearClientSecret ? "text" : "password"} value={self.state.client.secrets[0].value} onChange={self.handleChangeClientSecret} />                                
                        <FormHelperText>
                          	<Checkbox onChange={() => self.handleToggleValue('isClearClientSecret')} /><span>{t('revealClientSecret')}</span>
						</FormHelperText>
                    </FormControl>
                    {/* Client logo */}
                    <FormControl fullWidth={true} className={classes.margin}>
                        <InputLabel htmlFor="clientLogo">{t('clientLogo')}</InputLabel>
                        <Input id="clientLogo" value={self.state.client.logo_uri} name="logo_uri" onChange={self.handleChangeProperty}  />
                        <FormHelperText>{t('clientLogoDescription')}</FormHelperText>
                    </FormControl>
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
                    <Button variant="raised" color="primary" onClick={() => self.handleToggleValue('isAdvSettingsDisplayed')}>{t('showAdvancedSettings')}</Button>
                    {self.state.isAdvSettingsDisplayed && (
	                    <Paper className={classes.paper} >
	                    	<Tabs value={self.state.tabIndex} onChange={self.handleTabChange}>
	                        	<Tab label={t('clientGrantTypes')} />
                                <Tab label={t('clientOAuth')} />
	                        </Tabs>
                            {self.state.tabIndex === 0 && (
                                <ChipsSelector input={self.state.grantTypeSelectorOpts} />
                            )}
                            {self.state.tabIndex === 1 && (
                                <div>
                                    <FormControl fullWidth={true} className={classes.margin}>
                                        <InputLabel>{t('jsonWebTokenSignatureAlg')}</InputLabel>
                                        <Input value={self.state.client.id_token_signed_response_alg} name="id_token_signed_response_alg" onChange={self.handleChangeProperty}  />
                                        <FormHelperText>{t('jsonWebTokenSignatureAlgDescription')}</FormHelperText>
                                    </FormControl>
                                    <FormControl fullWidth={true} className={classes.margin}>
                                        <InputLabel>{t('tokenEndpointAuthMethod')}</InputLabel>
                                        <Input value={self.state.client.token_endpoint_auth_method} name="token_endpoint_auth_method" onChange={self.handleChangeProperty}  />
                                        <FormHelperText>{t('tokenEndpointAuthMethodDescription')}</FormHelperText>
                                    </FormControl>
                                </div>
                            )}
	                    </Paper>
                    )}
              	</div>
        );
    }
}

export default translate('common', { wait: process && !process.release })(withStyles(styles)(GeneralSettingsTab));