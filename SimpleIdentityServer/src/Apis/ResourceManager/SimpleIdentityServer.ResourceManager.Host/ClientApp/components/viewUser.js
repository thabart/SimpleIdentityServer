import React, { Component } from "react";
import { translate } from 'react-i18next';
import { NavLink } from "react-router-dom";
import { ResourceOwnerService } from '../services';
import AppDispatcher from '../appDispatcher';
import Constants from '../constants';
import Save from '@material-ui/icons/Save';

import { CircularProgress, IconButton, Select, MenuItem, Checkbox, Typography, Grid } from 'material-ui';
import Table, { TableBody, TableCell, TableHead, TableRow, TableFooter } from 'material-ui/Table';
import { FormControl, FormHelperText } from 'material-ui/Form';
import Input, { InputLabel } from 'material-ui/Input';
import { withStyles } from 'material-ui/styles';

const styles = theme => ({
  margin: {
    margin: theme.spacing.unit,
  }
});

class ViewUser extends Component {
    constructor(props) {
        super(props);
        this.refreshData = this.refreshData.bind(this);
        this.saveUser = this.saveUser.bind(this);
        this.handleChangeProperty = this.handleChangeProperty.bind(this);
        this.state = {
        	isLoading: true,
        	login: '',
        	user: {}
        };

	}

	/**
	* Display the user informations.
	*/
	refreshData() {
		var self = this;
		const { t } = self.props;
		self.setState({
			isLoading: true
		});
		ResourceOwnerService.get(self.state.login).then(function(ro) {
			console.log(ro);
			self.setState({
				isLoading: false,
				user: ro
			});
		}).catch(function() {
            self.setState({
                isLoading: false
            });
            AppDispatcher.dispatch({
                actionName: Constants.events.DISPLAY_MESSAGE,
                data: t('resourceOwnerCannotBeRetrieved')
            });			
		});
	}

	/**
	* Save the user.
	*/
	saveUser() {

	}

    /**
    * Change the property.
    */
    handleChangeProperty(e) {
        var self = this;
        var scope = self.state.scope;
        scope[e.target.name] = e.target.value;
        self.state.scope.claims = [];
        self.setState({
            scope: scope
        });
    }

    render() {
    	var self = this;
    	const { t, classes } = self.props;
    	return (<div className="block">
            <div className="block-header">
                <Grid container>
                    <Grid item md={5} sm={12}>
                        <h4>{t('resourceOwner')}</h4>
                        <i>{t('resourceOwnerDescription')}</i>
                    </Grid>
                    <Grid item md={7} sm={12}>                        
                        <ul className="breadcrumb float-md-right">
                            <li className="breadcrumb-item"><NavLink to="/">{t('websiteTitle')}</NavLink></li>
                            <li className="breadcrumb-item"><NavLink to="/resourceowners">{t('resourceOwners')}</NavLink></li>
                            <li className="breadcrumb-item">{t('resourceOwner')}</li>
                        </ul>
                    </Grid>
                </Grid>
            </div>
            <div className="card">
                <div className="header">
                	 <h4 style={{display: "inline-block"}}>{t('resourceOwnerInformation')}</h4>                   
                    <div style={{float: "right"}}>                        
                        <IconButton onClick={this.saveScope}>
                            <Save />
                        </IconButton>
                    </div>
                </div>
                <div className="body">
                	{ self.state.isLoading ? (<CircularProgress />) : (
                    	<Grid container spacing={40}>
                    		<Grid item md={5} sm={12}>
		                        {/* Login */}
		                        <FormControl fullWidth={true} className={classes.margin} disabled={self.props.isReadOnly}>
		                            <InputLabel>{t('roLogin')}</InputLabel>
		                            <Input value={self.state.user.login} name="login" onChange={self.handleChangeProperty}  />
		                            <FormHelperText>{t('roLoginDescription')}</FormHelperText>
		                        </FormControl>                   			
                    		</Grid>
                    		<Grid item md={5} sm={12}>
                    			{/* Claims */}
                    				<Table>
                    					<TableHead>
                                        	<TableCell></TableCell>
                                        	<TableCell>{t('claimKey')}</TableCell>
                                        	<TableCell>{t('claimValue')}</TableCell>
                    					</TableHead>
                                		<TableBody>

                                		</TableBody>
                    				</Table>
                    		</Grid>
                    	</Grid>
                    )}
                </div>
            </div>
    	</div>);
    }

    componentDidMount() {
    	var self = this;
    	self.setState({
    		login: self.props.match.params.id
    	}, function() {
    		self.refreshData();
    	});
    }
}

export default translate('common', { wait: process && !process.release })(withStyles(styles)(ViewUser));