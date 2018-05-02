import React, { Component } from "react";
import { NavLink } from "react-router-dom";
import { withRouter, Link } from 'react-router-dom';
import { translate } from 'react-i18next';
import { SessionService, EndpointService, ProfileService } from './services';
import Constants from './constants';
import AppDispatcher from './appDispatcher';

import { IconButton, Button , Drawer, Select, MenuItem, SwipeableDrawer, FormControl, Grid, CircularProgress, Snackbar, Divider, Avatar, Typography } from 'material-ui';
import  List, { ListItem, ListItemText, ListItemIcon } from 'material-ui/List';
import { InputLabel } from 'material-ui/Input';
import { withStyles } from 'material-ui/styles';
import ExpandLess from '@material-ui/icons/ExpandLess';
import ExpandMore from '@material-ui/icons/ExpandMore';
import Settings from '@material-ui/icons/Settings';
import Face from '@material-ui/icons/Face';
import Language from '@material-ui/icons/Language';
import Label from '@material-ui/icons/Label';
import Lock from '@material-ui/icons/Lock';
import Collapse from 'material-ui/transitions/Collapse';

const drawerWidth = 300;
const styles = theme => ({
  root: {
    display: 'flex'
  },
  body: {
    marginLeft: drawerWidth + "px"
  },
  drawerPaper: {
    width: drawerWidth
  },
  formControl: {
    margin: theme.spacing.unit,
    minWidth: 120,
  },
  nested: {
    paddingLeft: theme.spacing.unit * 4,
  },
  avatar: {
    width: 120,
    height: 120
  }
});

class Layout extends Component {
    constructor(props) {
        super(props);
        this._appDispatcher = null;
        this._sessionFrame = null;
        this._checkSessionInterval = null;
        this.disconnect = this.disconnect.bind(this);
        this.toggleValue = this.toggleValue.bind(this);
        this.navigate = this.navigate.bind(this);
        this.refresh = this.refresh.bind(this);
        this.startCheckSession = this.startCheckSession.bind(this);
        this.stopCheckSession = this.stopCheckSession.bind(this);
        this.handleSelection = this.handleSelection.bind(this);
        this.handleSaveChanges = this.handleSaveChanges.bind(this);
        this.handleSnackbarClose = this.handleSnackbarClose.bind(this);
        this.displayMessage = this.displayMessage.bind(this);
        this.state = {
            isManageOpenidServerOpened: false,
            isManageAuthServersOpened: false,
            isScimOpened: false,
            isLoggedIn: false,
            isOauthDisplayed: false,
            isScimDisplayed: false,
            isAuthDisplayed: false,
            isDrawerDisplayed: false,
            openidEndpoints: [],
            authEndpoints: [],
            scimEndpoints: [],
            selectedOpenid: null,
            selectedAuth: null,
            selectedScim: null,
            isLoading: false,
            isSnackbarOpened: false,
            snackbarMessage: '',
            user: {}
        };
    }
    /**
     * Disconnect the user.
     * @param {any} e
     */
    disconnect() {
        // TODO : Resolve this url.
        var url = "http://localhost:60000/end_session?post_logout_redirect_uri=http://localhost:64950/end_session&id_token_hint="+ SessionService.getSession().id_token;
        var w = window.open(url, 'targetWindow', 'toolbar=no,location=no,status=no,menubar=no,scrollbars=yes,resizable=yes,width=400,height=400');
        var interval = setInterval(function() {
            if (w.closed) {
                clearInterval(interval);
                return;
            }

            var href = w.location.href;
            if (href === "http://localhost:64950/end_session") {                
                clearInterval(interval);
                w.close();
            }
        });
    }

    toggleValue(menu) {    
        this.setState({
            [menu]: !this.state[menu]
        });
    }

    navigate(href) {
        this.props.history.push(href);
    }

    /**
    * Refresh the settings menu.
    */
    refresh() {
        var self = this;
        const { t } = self.props;
        self.setState({
            isLoading: true
        });

        EndpointService.getAll().then(function(endpoints) {
            var authEndpoints = endpoints.filter(function(endpoint) { return endpoint.type === 0; });
            var openidEndpoints = endpoints.filter(function(endpoint) { return endpoint.type === 1; });
            var scimEndpoints = endpoints.filter(function(endpoint) { return endpoint.type === 2; });
            ProfileService.get().then(function(profile) {
                self.setState({
                    authEndpoints: authEndpoints,
                    openidEndpoints: openidEndpoints,
                    scimEndpoints: scimEndpoints,
                    selectedOpenid: profile.openid_url,
                    selectedAuth: profile.auth_url,
                    selectedScim: profile.scim_url,
                    isLoading: false
                });
                AppDispatcher.dispatch({
                    actionName: Constants.events.SESSION_CREATED,
                    data: profile
                });
            }).catch(function() {
                var selectedOpenid = null;
                var selectedAuth = null;
                var selectedScim = null;
                if (authEndpoints.length > 0) {
                    selectedAuth = authEndpoints[0].url;
                }

                if (openidEndpoints.length > 0) {
                    selectedOpenid = openidEndpoints[0].url;
                }

                if (scimEndpoints.length > 0) {
                    selectedScim = scimEndpoints[0].url;
                }

                self.setState({
                    authEndpoints: authEndpoints,
                    openidEndpoints: openidEndpoints,
                    scimEndpoints: scimEndpoints,
                    selectedOpenid: selectedOpenid,
                    selectedAuth: selectedAuth,
                    selectedScim: selectedScim,
                    isLoading: false
                });
                AppDispatcher.dispatch({
                    actionName: Constants.events.SESSION_CREATED,
                    data: {
                        openid_url: selectedOpenid,
                        auth_url: selectedAuth,
                        scim_url: selectedScim
                    }
                });
            });
        }).catch(function() {
            self.setState({
                isLoading: false
            });
            AppDispatcher.dispatch({
                actionName: Constants.events.DISPLAY_MESSAGE,
                data: t('profileCannotBeRetrieved')
            });
        });

        var image = "/img/unknown.png";
        var givenName = t("unknown");
        var session = SessionService.getSession();
        if (session && session.id_token) {
            var idToken = session.id_token;
            var splitted = idToken.split('.');
            var claims = JSON.parse(window.atob(splitted[1]));
            if (claims.picture) {
                image = claims.picture;
            }

            if (claims.given_name) {
                givenName = claims.given_name;
            }
        }

        self.setState({
            user: {
                name: givenName,
                picture: image
            }
        });
    }

    /**
    * Start the check the session.
    */
    startCheckSession() {
        var self = this;
        if (self._checkSessionInterval) {
            return;
        }

        var evt = window.addEventListener("message", function(e) {
            if (e.data !== 'unchanged') {
                AppDispatcher.dispatch({
                    actionName: Constants.events.USER_LOGGED_OUT
                });
                self.props.history.push('/');
            }
        }, false);
        var originUrl = window.location.protocol + "//" + window.location.host;
        self._checkSessionInterval = setInterval(function() { 
            var session = SessionService.getSession();
            var message = "ResourceManagerClientId ";
            if (session) {
                message += session.sessionState;
            } else {
                session += "tmp";
            }

            var win = self._sessionFrame.contentWindow;
            // TODO : Externalize the client_id & openid url.
            win.postMessage(message, "http://localhost:60000");
        }, 3*1000);
    }

    /**
    * Stop to check the session.
    */
    stopCheckSession() {
        var self = this;
        if (!self._checkSessionInterval) {
            return;
        }

        clearInterval(self._checkSessionInterval);
        self._checkSessionInterval = null;
    }

    /**
    * Handle the selection.
    */
    handleSelection(e) {
        this.setState({
            [e.target.name]: e.target.value
        });
    }

    /**
    *   Save the profile.
    */
    handleSaveChanges() {
        var self = this;
        const { t } = self.props;
        var request = {
            openid_url: self.state.selectedOpenid,
            auth_url: self.state.selectedAuth,
            scim_url: self.state.selectedScim
        };
        self.setState({
            isLoading: true
        });
        ProfileService.update(request).then(function() {
            self.setState({
                isLoading: false,
                isDrawerDisplayed: false
            });
            AppDispatcher.dispatch({
                actionName: Constants.events.DISPLAY_MESSAGE,
                data: t('profileSaved')
            });
            AppDispatcher.dispatch({
                actionName: Constants.events.SESSION_UPDATED,
                data: request
            });
        }).catch(function() {
            AppDispatcher.dispatch({
                actionName: Constants.events.DISPLAY_MESSAGE,
                data: t('profileCannotBeSaved')
            });
            self.setState({
                isLoading: false
            });
        });
    }

    /**
    * Snackbar is closed.
    */
    handleSnackbarClose(message) {
        this.setState({
            isSnackbarOpened: false
        });
    }

    /**
    * Display the message in the snackbar.
    */
    displayMessage(message) {
        this.setState({
            isSnackbarOpened: true,
            snackbarMessage: message
        });
    }

    render() {
        var self = this;
        const { t, classes } = this.props;
        var openidEndpoints = [];
        var authEndpoints = [];
        var scimEndpoints = [];
        if (self.state.openidEndpoints) {
            self.state.openidEndpoints.forEach(function(openidEndpoint) {
                openidEndpoints.push((<MenuItem key={openidEndpoint.name} value={openidEndpoint.url}>{openidEndpoint.description}</MenuItem>));
            });
        }

        if (self.state.authEndpoints) {
            self.state.authEndpoints.forEach(function(authEndpoint) {
                authEndpoints.push((<MenuItem key={authEndpoint.name} value={authEndpoint.url}>{authEndpoint.description}</MenuItem>));
            });
        }

        if (self.state.scimEndpoints) {
            self.state.scimEndpoints.forEach(function(scimEndpoint) {
                scimEndpoints.push((<MenuItem key={scimEndpoint.name} value={scimEndpoint.url}>{scimEndpoint.description}</MenuItem>))
            });
        }

        return (
        <div>
            <SwipeableDrawer open={self.state.isDrawerDisplayed} anchor="right" onClose={ () => self.setState({ isDrawerDisplayed: false }) } onOpen={ () => self.setState({ isDrawerDisplayed: true }) }>
                {self.state.isLoading ? (<CircularProgress />) : (
                    <div style={{padding: "20px"}}>
                        <div>
                            {openidEndpoints.length > 0 && (
                                <div>
                                    <FormControl fullWidth={true} className={classes.formControl}>
                                        <InputLabel>{t('selectedPreferredOpenIdProvider')}</InputLabel>
                                        <Select value={self.state.selectedOpenid} name='selectedOpenid' onChange={self.handleSelection}>
                                            {openidEndpoints}
                                        </Select>
                                    </FormControl>
                                </div>
                            )}
                            {authEndpoints.length > 0 && (
                                <div>
                                    <FormControl fullWidth={true} className={classes.formControl}>
                                        <InputLabel>{t('selectPreferredAuthorizationServer')}</InputLabel>
                                        <Select  value={self.state.selectedAuth} name='selectedAuth' onChange={self.handleSelection}>
                                            {authEndpoints}
                                        </Select>
                                    </FormControl>
                                </div>
                            )}
                            {scimEndpoints.length > 0 && (
                                <div>
                                    <FormControl fullWidth={true} className={classes.formControl}>
                                        <InputLabel>{t('selectPreferredScimServer')}</InputLabel>                            
                                        <Select value={self.state.selectedScim} name='selectedScim' onChange={self.handleSelection}>
                                            {scimEndpoints}
                                        </Select>
                                    </FormControl>
                                </div>
                            )}
                            <Button  variant="raised" color="primary" onClick={self.handleSaveChanges}>{t('saveChanges')}</Button>
                        </div>
                    </div>
                )}
            </SwipeableDrawer>
            <Drawer docked={true} variant="permanent" anchor="left" classes={{ paper: classes.drawerPaper }}>                
                <List>
                    {(self.state.isLoggedIn && (
                        <ListItem>
                            <div style={{ width: "100%", "textAlign": "center"}}>
                                <img src={self.state.user.picture} style={{"width": "80px", "height": "80px"}} className="img-circle img-thumbnail" />
                                <Typography variant="title">{self.state.user.name}</Typography>
                            </div>
                        </ListItem>
                    ))}
                    {(self.state.isLoggedIn && (
                        <Divider />
                    ))}
                    {/* About menu item */}
                    <ListItem button onClick={() => self.navigate('/about')}>
                        <ListItemText>{t('aboutMenuItem')}</ListItemText>
                    </ListItem>
                    {/* Dashboard menu item */}                        
                    {(self.state.isLoggedIn && !process.env.IS_MANAGE_DISABLED && (
                        <ListItem button onClick={() => self.navigate('/dashboard')}>
                            <ListItemText>{t('dashboardMenuItem')}</ListItemText>
                        </ListItem>
                    ))}
                    {/* Openid menu item */}                        
                    {(self.state.isLoggedIn && !process.env.IS_MANAGE_DISABLED && (
                        <ListItem button onClick={() => self.toggleValue('isManageOpenidServerOpened')}>
                            <ListItemText>{t('manageOpenidServers')}</ListItemText>
                            { this.state.isManageOpenidServerOpened ? <ExpandLess /> : <ExpandMore /> }
                        </ListItem>
                    ))}
                    {(self.state.isLoggedIn && !process.env.IS_MANAGE_DISABLED && (
                        <Collapse in={this.state.isManageOpenidServerOpened}>
                            <List>
                                <ListItem className={classes.nested} button onClick={() => self.navigate('/resourceowners')}>
                                    <ListItemIcon><Face /></ListItemIcon>
                                    <ListItemText>{t('resourceOwners')}</ListItemText>
                                </ListItem>
                                <ListItem className={classes.nested} button onClick={() => self.navigate('/openidclients')}>
                                    <ListItemIcon><Language /></ListItemIcon>
                                    <ListItemText>{t('openidclients')}</ListItemText>
                                </ListItem>
                                <ListItem className={classes.nested} button onClick={() => self.navigate('/openidscopes')}>
                                    <ListItemIcon><Label /></ListItemIcon>
                                    <ListItemText>{t('openidScopes')}</ListItemText>
                                </ListItem>
                            </List>
                        </Collapse>   
                    ))}       
                    {/* Authorisation server */}
                    {(this.state.isLoggedIn && !process.env.IS_MANAGE_DISABLED && (
                        <ListItem button onClick={() => self.toggleValue('isManageAuthServersOpened')}>
                            <ListItemText>{t('manageAuthServers')}</ListItemText>
                            { this.state.isManageAuthServersOpened ? <ExpandLess /> : <ExpandMore /> }
                        </ListItem>
                    ))}
                    {(this.state.isLoggedIn && !process.env.IS_MANAGE_DISABLED && (
                        <Collapse in={this.state.isManageAuthServersOpened}>
                            <List>
                                <ListItem className={classes.nested} button onClick={() => self.navigate('/authclients')}>
                                    <ListItemIcon><Language /></ListItemIcon>
                                    <ListItemText>{t('oauthClients')}</ListItemText>
                                </ListItem>
                                <ListItem className={classes.nested} button onClick={() => self.navigate('/authScopes')}>
                                    <ListItemIcon><Label /></ListItemIcon>
                                    <ListItemText>{t('authscopes')}</ListItemText>
                                </ListItem>
                                <ListItem className={classes.nested} button onClick={() => self.navigate('/resources')}>
                                    <ListItemIcon><Lock /></ListItemIcon>
                                    <ListItemText>{t('resources')}</ListItemText>
                                </ListItem>
                            </List>
                        </Collapse>
                    ))}
                    {/* SCIM server */}
                    {(this.state.isLoggedIn && !process.env.IS_MANAGE_DISABLED && (
                        <ListItem button onClick={() => self.toggleValue('isScimOpened')}>
                            <ListItemText>{t('manageScimServers')}</ListItemText>
                            { this.state.isScimOpened ? <ExpandLess /> : <ExpandMore /> }
                        </ListItem>
                    ))}
                    {(this.state.isLoggedIn && !process.env.IS_MANAGE_DISABLED && (
                        <Collapse in={this.state.isScimOpened}>
                            <List>
                                <ListItem className={classes.nested} button><ListItemText>{t('scimSchemas')}</ListItemText></ListItem>
                                <ListItem className={classes.nested} button><ListItemText>{t('scimResources')}</ListItemText></ListItem>
                            </List>
                        </Collapse>
                    ))}
                    {/* Logs */}         
                    {!process.env.IS_LOG_DISABLED && this.state.isLoggedIn  && (
                       <ListItem button onClick={() => self.navigate('/logs')}>
                            <ListItemText>{t('logsMenuItem')}</ListItemText>
                        </ListItem>
                    )}
                    {/* Connect or disconnect */}
                    {(this.state.isLoggedIn ? (
                        <ListItem button onClick={() => self.disconnect()}>
                            <ListItemText>{t('disconnectMenuItem')}</ListItemText>
                        </ListItem>
                    ) : (                        
                        <ListItem button onClick={() => self.navigate('/login')}>
                            <ListItemText>{t('connectMenuItem')}</ListItemText>
                        </ListItem>
                    ))}
                </List>
            </Drawer>
            <section className={classes.body}>
                { /* Navigation */ }
                <nav className="navbar navbar-toggleable-md">
                    <a className="navbar-brand" href="#" id="uma-title">{t('websiteTitle')}</a>
                    <ul className="navbar-nav mr-auto">
                    </ul>
                    {this.state.isLoggedIn && (<IconButton  onClick={() => self.toggleValue('isDrawerDisplayed')}><Settings button  /></IconButton>)}
                </nav>
                { /* Display component */}
                <section id="body">
                    {this.props.children}
                </section>
            </section>
            { this.state.isLoggedIn && (<div>
                    <iframe ref={(elt) => { self._sessionFrame = elt; self.startCheckSession(); }} id="session-frame" src="http://localhost:60000/check_session" style={{display: "none"}} /> 
                </div>
            )}
            <Snackbar anchorOrigin={{ vertical: 'bottom', horizontal: 'center' }} open={self.state.isSnackbarOpened} onClose={self.handleSnackbarClose} message={<span>{self.state.snackbarMessage}</span>} />
        </div>);
    }

    componentDidMount() {
        var self = this;
        self._appDispatcher = AppDispatcher.register(function (payload) {
            switch (payload.actionName) {
                case Constants.events.USER_LOGGED_IN:
                    self.setState({
                        isLoggedIn: true
                    });
                    self.refresh();
                    break;
                case Constants.events.USER_LOGGED_OUT:
                    self.setState({
                        isLoggedIn: false
                    });
                    SessionService.remove();
                    break;
                case Constants.events.DISPLAY_MESSAGE:
                    self.displayMessage(payload.data);
                    break;
            }
        });

        var isLoggedIn = !SessionService.isExpired();
        if (isLoggedIn) {
            AppDispatcher.dispatch({
                actionName: Constants.events.USER_LOGGED_IN
            });
        }        
    }

    componentWillUnmount() {
        AppDispatcher.unregister(this._appDispatcher);
    }
}

export default translate('common', { wait: process && !process.release })(withRouter(withStyles(styles)(Layout)));