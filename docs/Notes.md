# Notes for future development

## User-Role relationships
I've been thinking about how to keep the User-Role relationship
up to date. There are complications with supporting groups of
groups. 

It is much simpler if groups can only hold users, not other groups. 
In this case, we can have a connecting table such as the following

*user_role_relation*
* user_id
* role_id
* group_id (nullable)

Here are some simple business rules
* If the user is directly assigned to the role, the group_id is null. 
* If a user is removed from a group, delete where user_id and group_id match
* If a user is removed from a role, delete where user_id and role_id match
* If any relation exists between user_id and role_id, the user is in the role

## Development
This needs to be tested and verified. This needs to be handled on the Query 
side of CQRS. I shall introduce a Sqlite in-memory database to handle testing.

Create a new actor, which subscribes to these events. A good example of 
subscribing to an event can be found in `RequestReplyActor.fs`. 

    actor |> SubjectActor.subscribeTo actors.Events


