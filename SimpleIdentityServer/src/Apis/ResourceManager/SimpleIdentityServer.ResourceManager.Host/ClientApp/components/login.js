import React, { Component } from "react";
import AppDispatcher from '../appDispatcher';
import Constants from '../constants';
import { WebsiteService, SessionService } from '../services';
import { withRouter } from 'react-router-dom';
import { translate } from 'react-i18next';
import CubeLoading from './cubeLoading';

import { TextField , RaisedButton } from 'material-ui';

class Login extends Component {
    constructor(props) {
        super(props);
        this.handleInputChange = this.handleInputChange.bind(this);
        this.authenticate = this.authenticate.bind(this);
        this.externalAuthenticate = this.externalAuthenticate.bind(this);
        this.state = {
            login           : null,
            password        : null,
            errorMessage    : null,
            isLoading       : false
        };
    }
    handleInputChange(e) {
        const target = e.target;
        const value = target.type === 'checkbox' ? target.checked : target.value;
        const name = target.name;
        this.setState({
            [name]: value
        });
    }

    parseJwt(token) {
        var base64Url = token.split('.')[1];
        var base64 = base64Url.replace('-', '+').replace('_', '/');
        return JSON.parse(window.atob(base64));
    }

    /**
     * Authenticate the user with login and password.
     * @param {any} e
     */
    authenticate(e) {
        e.preventDefault();
        var self = this;
        self.setState({
            isLoading: true
        });
        const { t } = self.props;
        WebsiteService.authenticate(self.state.login, self.state.password).then(function (data) {
            var payload = self.parseJwt(data.id_token);
            if (!payload.role || payload.role !== 'administrator') {
                self.setState({
                    errorMessage: t('notAdministrator'),
                    isLoading: false
                });
                return;
            }

            SessionService.setSession(data);
            AppDispatcher.dispatch({
                actionName: Constants.events.USER_LOGGED_IN
            });
            self.props.history.push('/');
            self.setState({
                isLoading: false
            });
        }).catch(function (e) {
            self.setState({
                errorMessage: e.responseJSON.error_description,
                isLoading: false
            });
        });
    }

    /**
    * External authentication.
    */
    externalAuthenticate() {
        var getParameterByName = function (name, url) {
            if (!url) url = window.location.href;
            name = name.replace(/[\[\]]/g, "\\$&");
            var regex = new RegExp("[?&]" + name + "(=([^&#]*)|&|#|$)"),
                results = regex.exec(url);
            if (!results) return null;
            if (!results[2]) return '';
            return decodeURIComponent(results[2].replace(/\+/g, " "));
        };        
        const {t} = this.props;
        var self = this;
        self.setState({
            isLoading: true
        });
        // TODO : Externalize this dependency.
        var url = "http://localhost:60000/authorization?scope=openid role profile&state=75BCNvRlEGHpQRCT&redirect_uri=http://localhost:64950/callback&response_type=id_token token&client_id=ResourceManagerClientId&nonce=nonce&response_mode=query";
        var w = window.open(url, 'targetWindow', 'toolbar=no,location=no,status=no,menubar=no,scrollbars=yes,resizable=yes,width=400,height=400');        
        var interval = setInterval(function () {
            if (w.closed) {
                clearInterval(interval);
                return;
            }

            var href = w.location.href;
            var idToken = getParameterByName('id_token', href);
            var accessToken = getParameterByName('access_token', href);
            var sessionState = getParameterByName('session_state', href);
            if (!idToken && !accessToken && !sessionState) {
                return;
            }

            sessionState = sessionState.replace(' ', '+');
            var payload = self.parseJwt(idToken);
            if (!payload.role || payload.role !== 'administrator') {
                self.setState({
                    errorMessage: t('notAdministrator'),
                    isLoading: false
                });
                clearInterval(interval);
                w.close();
                return;
            }

            var session = {
                token: accessToken,
                id_token: idToken,
                sessionState: sessionState
            };
            SessionService.setSession(session);
            AppDispatcher.dispatch({
                actionName: Constants.events.USER_LOGGED_IN
            });
            self.props.history.push('/');
            self.setState({
                isLoading: false
            });
            clearInterval(interval);
            w.close();
        });
    }

    render() {
        const { t } = this.props;
        return (<div className="block">
            <div className="block-header">
                <h4>{t('loginTitle')}</h4>
            </div>
            <div className="container-fluid">
                <div className="row">
                    <div className="col-md-12">
                        <div className="card">
                            <div className="body">
                                <form onSubmit={this.authenticate}>
                                    <div className="form-group">
                                        <TextField hintText={t('loginHintText')} name="login" onChange={this.handleInputChange} fullWidth={true}/>
                                    </div>
                                    <div className="form-group">
                                        <TextField hintText={t('passwordHintText')} name="password" type="password" onChange={this.handleInputChange} fullWidth={true}/>
                                    </div>
                                    <RaisedButton label={t('connect')} primary={true} type="submit" />
                                </form>
                                <RaisedButton label={t('externalConnect')} primary={true} onClick={this.externalAuthenticate} />
                                {(this.state.errorMessage !== null && (
                                    <div className="alert alert-danger alert-dismissable" style={{ marginTop: '5px' }}>
                                        <strong>Danger !</strong> {this.state.errorMessage}
                                    </div>
                                ))}
                            </div>
                        </div>
                    </div>
                </div>
            </div>
        </div>);
    }
}

export default translate('common', { wait: process && !process.release })(withRouter(Login));