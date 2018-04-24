import React, { Component } from "react";
import { translate } from 'react-i18next';
import { ResourceOwnerService } from '../services';

import { Popover, IconButton, Menu, MenuItem } from 'material-ui';
import ReactTable from 'react-table'


const columns = [
    { Header: 'login', accessor: 'login' }
];

class ResourceOwners extends Component {
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
        var request = { start_index: startIndex, count: state.pageSize };
        if (state.filtered && state.filtered.length > 0) {
            var firstFilter = state.filtered[0];
            request.subjects = [ firstFilter.value ];
        }

        ResourceOwnerService.search(request, self.props.type).then(function (result) {
            var data = [];
            if (result.content) {
                result.content.forEach(function (client) {
                    data.push({
                        login: client['login']
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
                <h4>{t('resourceOwners')}</h4>
                <i>{t('resourceOwnersShortDescription')}</i>
            </div>
            <div className="container-fluid">
                <div className="row">
                    <div className="col-md-12">
                        <div className="card">
                            <div className="header">
                                <h4 style={{display: "inline-block"}}>{t('listOfRos')}</h4>
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
                                            <MenuItem primaryText={t('addResourceOwner')} />
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
                                    filterable={true}
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

export default translate('common', { wait: process && !process.release })(ResourceOwners);