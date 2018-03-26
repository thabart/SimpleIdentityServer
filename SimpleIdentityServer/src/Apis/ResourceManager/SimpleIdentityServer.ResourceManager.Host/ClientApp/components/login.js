import React, { Component } from "react";
import AppDispatcher from '../appDispatcher';
import Constants from '../constants';
import { WebsiteService, SessionService } from '../services';
import { withRouter } from 'react-router-dom';
import { translate } from 'react-i18next';
import CubeLoading from './cubeLoading';

class Login extends Component {
    constructor(props) {
        super(props);
        this.handleInputChange = this.handleInputChange.bind(this);
        this.authenticate = this.authenticate.bind(this);
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
    render() {
        const { t } = this.props;
        return (<div>
            <h4>{t('loginTitle')}</h4>
            {(this.state.isLoading ? (<CubeLoading />): (
                <div>
                    <form onSubmit={this.authenticate}>
                        <div className="form-group">
                            <input placeholder={t('login')} type="text" className="form-control" name="login" onChange={this.handleInputChange} />
                        </div>
                        <div className="form-group">
                            <input placeholder={t('password')} type="password" className="form-control" name="password" onChange={this.handleInputChange} />
                        </div>
                        <button type="submit" className="btn btn-purple full-width">{t('connect')}</button>
                    </form>
                    {(this.state.errorMessage !== null && (
                        <div className="alert alert-danger alert-dismissable" style={{ marginTop: '5px' }}>
                            <strong>Danger !</strong> {this.state.errorMessage}
                        </div>
                    ))}
                </div>
            ))}
         </div>);
    }
}

export default translate('common', { wait: process && !process.release })(withRouter(Login));