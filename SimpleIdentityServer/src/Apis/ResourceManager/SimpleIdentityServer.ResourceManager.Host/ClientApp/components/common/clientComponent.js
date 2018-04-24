import React, { Component } from "react";
import { translate } from 'react-i18next';
import { ClientService } from '../../services';

import { Popover, IconButton, Menu, MenuItem } from 'material-ui';
import ReactTable from 'react-table'


const columns = [
    {
        Header: '', accessor: 'logo_uri', Cell: row => (
            <img src={row.value} width="50" />
        )
    },
    { Header: 'Name', accessor: 'client_name' },
    { Header: 'Identifier', accessor: 'client_id' }
];

class ClientComponent extends Component {
    constructor(props) {
        super(props);
        this.displayPopOver = this.displayPopOver.bind(this);
        this.toggleProperty = this.toggleProperty.bind(this);
        this.fetchData = this.fetchData.bind(this);
        this.state = {
            data: [],
            isLoading: false,
            pages: 0,
            isSettingsOpened: false,
            anchorEl: null,
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

    fetchData(state, instance) {
        var self = this;
        self.setState({
            isLoading: true
        });
        var startIndex = state.page * state.pageSize;
        ClientService.search({ start_index: startIndex, count: state.pageSize }, self.props.type).then(function (result) {
            var data = [];
            if (result.content) {
                result.content.forEach(function (client) {
                    data.push({
                        logo_uri: client['logo_uri'],
                        client_name: client['client_name'],
                        client_id: client['client_id']
                    });
                });
            }
            
            var pages = Math.round((result.count + state.pageSize - 1) / state.pageSize);
            self.setState({
                isLoading: false,
                data: data,
                pages: pages
            });
        }).catch(function (e) {
            self.setState({
                isLoading: false,
                data: [],
                pages: 0
            });
        });
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
                                <ReactTable
                                    data={this.state.data}
                                    loading={this.state.isLoading}
                                    onFetchData={this.fetchData}
                                    pages={this.state.pages}
                                    columns={columns}
                                    defaultPageSize={10}
                                    manual={true}
                                    filterable={false}
                                    showPaginationTop={true}
                                    showPaginationBottom={false}
                                    sortable={false}
                                    className="-striped -highlight"
                                />
                            </div>
                        </div>
                    </div>
                </div>
            </div>
        </div>);
    }
}

export default translate('common', { wait: process && !process.release })(ClientComponent);