import React, { Component } from "react";
import { translate } from 'react-i18next';

import { Popover, IconButton, Menu, MenuItem } from 'material-ui';

class OAuthClients extends Component {
    constructor(props) {
        super(props);
        this.displayPopOver = this.displayPopOver.bind(this);
        this.toggleProperty = this.toggleProperty.bind(this);
        this.state = {
            clients : [],
            isSettingsOpened: false,
            anchorEl: null
        };
    }

    displayPopOver(e) {
        this.setState({
            isSettingsOpened: true,
            anchorEl: e.currentTarget
        })
    }

    toggleProperty(name) {
        this.setState({
            [name]: !this.state[name]
        })
    }

    render() {
        var self = this;
        const { t } = this.props;
        return (<div className="block">
            <div className="block-header">
                <h4>{t('oauthClients')}</h4>
                <i>{t('oauthClientsShortDescription')}</i>
            </div>
            <div className="container-fluid">
                <div className="row">
                    <div className="col-md-12">
                        <div className="card">
                            <div className="header">
                                <h4 style={{display: "inline-block"}}>{t('listOfClients')}</h4>
                                <div style={{float: "right"}}>
                                    <IconButton iconClassName="material-icons" onClick={self.displayPopOver}>&#xE5D4;</IconButton>                                    
                                    <Popover
                                      open={this.state.isSettingsOpened}
                                      anchorEl={this.state.anchorEl}
                                      anchorOrigin={{horizontal: 'left', vertical: 'bottom'}}
                                      targetOrigin={{horizontal: 'left', vertical: 'top'}}
                                    onRequestClose={() => self.toggleProperty('isSettingsOpened')}
                                    >   
                                        <Menu>
                                            <MenuItem primaryText={t('addClient')} />
                                        </Menu>
                                    </Popover>
                                </div>
                            </div>
                            <div className="body">
                                {t('content')}
                            </div>
                        </div>
                    </div>
                </div>
            </div>
        </div>);
    }

    componentDidMount() {
    }
}

export default translate('common', { wait: process && !process.release })(OAuthClients);