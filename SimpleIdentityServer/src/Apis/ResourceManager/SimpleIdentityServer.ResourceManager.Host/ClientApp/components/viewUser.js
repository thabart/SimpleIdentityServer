import React, { Component } from "react";
import { translate } from 'react-i18next';
import { NavLink } from "react-router-dom";
import { ResourceOwnerService } from '../services';
import AppDispatcher from '../appDispatcher';
import Constants from '../constants';
import Save from '@material-ui/icons/Save';

import { CircularProgress, IconButton, Select, MenuItem, Checkbox, Typography, Grid } from 'material-ui';

class ViewUser extends Component {
    constructor(props) {
        super(props);
        this.refreshData = this.refreshData.bind(this);
        this.saveUser = this.saveUser.bind(this);
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

    render() {
    	var self = this;
    	const { t } = self.props;
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
                    	<div>

                    	</div>
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

export default translate('common', { wait: process && !process.release })(ViewUser);