import React, { Component } from "react";
import { translate } from 'react-i18next';
import { NavLink, withRouter } from 'react-router-dom';
import { ScopeService } from '../services';
import { CircularProgress, IconButton, Grid } from 'material-ui';
import { DisplayScope } from './common';
import Save from '@material-ui/icons/Save';
import AppDispatcher from '../appDispatcher';
import Constants from '../constants';

class AddScope extends Component {
    constructor(props) {
        super(props);
        this.saveScope = this.saveScope.bind(this);
        this.state = {
            type: '',
        	scope: {
                type: 0,
                claims: []                
            },
        	isLoading: true
        };
    }

    /**
    * Save the scope.
    */
    saveScope() {
        var self = this;
        self.setState({
            isLoading: true
        });
        ScopeService.add(self.state.scope, self.state.type).then(function() {
            self.setState({
                isLoading: false
            });
            self.props.history.push(self.state.type === "openid" ? "/openidscopes" : "/authscopes");
            AppDispatcher.dispatch({
                actionName: Constants.events.DISPLAY_MESSAGE,
                data: t('scopeAdded')
            });
        }).catch(function(e) {
            self.setState({
                isLoading: false
            });
            AppDispatcher.dispatch({
                actionName: Constants.events.DISPLAY_MESSAGE,
                data: t('scopeCannotBeAdded')
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
                <Grid container>
                    <Grid item md={5} sm={12}>
                        <h4>{t('scopeTitle')}</h4>
                        <i>{t('scopeShortDescription')}</i>
                    </Grid>
                    <Grid item md={7} sm={12}>                        
                        <ul className="breadcrumb float-md-right">
                            <li className="breadcrumb-item"><NavLink to="/">{t('websiteTitle')}</NavLink></li>
                            <li className="breadcrumb-item"><NavLink to={self.state.type === "openid" ? "/openidscopes" : "/authscopes"}>{t('scopes')}</NavLink></li>
                            <li className="breadcrumb-item">{t('scope')}</li>
                        </ul>
                    </Grid>
                </Grid>
            </div>
            <div className="card">
                <div className="header">
                    <h4 style={{display: "inline-block"}}>{t('addScope')}</h4>                    
                    <div style={{float: "right"}}>                        
                        <IconButton onClick={this.saveScope}>
                            <Save />
                        </IconButton>
                    </div>
                </div>
                <div className="body">
                    { self.state.isLoading ? (<CircularProgress />) : (
                        <DisplayScope type={self.state.type} isReadOnly={false} scope={self.state.scope} />
                    ) }
                </div>
            </div>
        </div>);
    }

    componentDidMount() {
        var self = this;
        self.setState({
            type: self.props.match.params.type
        }, () => {
            self.setState({
                isLoading: false
            });
        });
    }
}

export default translate('common', { wait: process && !process.release })(withRouter(AddScope));