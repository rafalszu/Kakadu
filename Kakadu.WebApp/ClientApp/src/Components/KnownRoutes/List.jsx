import React from "react";
import { connect } from "react-redux";
import { Item } from './Item';
import { Replies } from './Replies'

class List extends React.Component {
    render() {
        return (
            <div className={this.props.className}>
                <div className="col-md-4">
                    <ul className="list-group mb-3">
                        {this.props.knownRoutes && this.props.knownRoutes.map((route, index) => 
                            <Item key={route.id} route={route} />
                        )}
                    </ul>
                </div>
                <div className="col-md-4">
                    <Replies />
                </div>
            </div>
        )
    }
}

function mapStateToProps(state) {
    const { services } = state;
    const { item } = services;
    let knownRoutes = item && item.knownRoutes;
    return {
        knownRoutes
    };
}

const connectedKnownRoutesPage = connect(mapStateToProps)(List);
export { connectedKnownRoutesPage as List };